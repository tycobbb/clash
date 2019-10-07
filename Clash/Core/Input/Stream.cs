using Clash.Maths;

namespace Clash.Input {
  using Ext;
  using K = Config;

  // -- interfaces --
  public interface IStream {
    Snapshot Get(uint offset);
    Snapshot GetCurrent();
  }

  public interface IMutableStream: IStream {
    void OnUpdate(float time);
  }

  // -- impls --
  public sealed class Stream: IMutableStream {
    // -- dependencies --
    private readonly ISource Source;

    // -- properties --
    private readonly Buffer buffer;

    // -- lifetime --
    public Stream(ISource source, Snapshot initial = default) {
      Source = source;
      buffer = new Buffer(10);
      buffer.Add(initial);
    }

    // -- IStream --
    public Snapshot Get(uint offset) {
      return buffer[offset];
    }

    public Snapshot GetCurrent() {
      return Get(0);
    }

    // -- IMutableStream --
    public void OnUpdate(float time) {
      buffer.Add(CaptureSnapshot(time));
    }

    // -- queries --
    private Snapshot CaptureSnapshot(float time) {
      return new Snapshot(
        CaptureMove(time),
        CaptureButton("JumpA"),
        CaptureButton("JumpB"),
        CaptureButton("ShieldL"),
        CaptureButton("ShieldR"),
        time
      );
    }

    private Analog CaptureMove(float time) {
      var current = GetCurrent();
      var prevTime = current.Time;
      var prevMove = current.Move;

      // get the raw stick position
      var raw = new Vec(
        Source.GetAxis("MoveX"),
        Source.GetAxis("MoveY")
      );

      var pos = raw;

      // filter raw position
      if (pos.Mag() <= K.DeadZone) {
        pos = Vec.Zero;
      }

      // determine primary direction
      Direction direction;
      if (pos.X == 0.0f && pos.Y == 0.0f) {
        direction = Direction.Neutral;
      } else if (Mathf.Abs(pos.X) > Mathf.Abs(pos.Y)) {
        direction = pos.X > 0.0f ? Direction.Right : Direction.Left;
      } else {
        direction = pos.Y > 0.0f ? Direction.Up : Direction.Down;
      }

      // calc instantaneous stick speed
      var delta = raw - prevMove.RawPosition;
      var speed = Mathf.Abs(delta.Mag() / (time - prevTime));

      // calc the magnitude of the primary direction
      var mag = Mathf.Abs(direction.IsHorizontal() ? pos.X : pos.Y);

      // determine analog state
      var state = prevMove.State;

      // if the stick is neutral
      if (direction.IsNeutral()) {
        if (state == StateA.Idle || speed < K.IdleSpeed) {
          state = StateA.Idle;
        } else {
          state = StateA.Unknown;
        }
      }
      // if we're moving in the same direction
      else if (direction == prevMove.Direction) {
        if (state == StateA.Active || state == StateA.Switch || state == StateA.SwitchTap) {
          state = StateA.Active;
        } else if (speed >= K.TapSpeed && mag == 1.0f) {
          state = StateA.SwitchTap;
        } else if (speed < K.TapSpeed) {
          state = StateA.Switch;
        } else {
          state = StateA.Unknown;
        }
      }
      // if we switched directions this frame
      else {
        if (speed >= K.TapSpeed && mag == 1.0f) {
          state = StateA.SwitchTap;
        } else if (speed >= K.TapSpeed) {
          state = StateA.Unknown;
        } else {
          state = StateA.Switch;
        }
      }

      if (state != prevMove.State) {
        Log.Verbose($"[Input.Stream] SwitchState({prevMove.State} => {state}, speed: {speed}, mag: {mag})");
      }

      return new Analog(state, direction, pos, raw);
    }

    private Button CaptureButton(string name) {
      StateB state;
      if (Source.GetButtonDown(name)) {
        state = StateB.Down;
      } else if (Source.GetButtonUp(name)) {
        state = StateB.Up;
      } else {
        state = Source.GetButton(name) ? StateB.Active : StateB.Inactive;
      }

      return new Button(state);
    }
  }
}
