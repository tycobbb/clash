using Clash.Maths;

namespace Clash.Input {
  using Ext;
  using K = Config;

  // -- interfaces --
  public interface IStream {
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
    private Snapshot current;

    // -- lifetime --
    public Stream(ISource source) {
      Source = source;
    }

    // -- IStream --
    public Snapshot GetCurrent() {
      return current;
    }

    // -- IMutableStream --
    public void OnUpdate(float time) {
      current = CaptureSnapshot(time);
    }

    // -- queries --
    private Snapshot CaptureSnapshot(float time) {
      return new Snapshot(
        CaptureMove(time),
        CaptureButton("JumpA"),
        CaptureButton("JumpB"),
        time
      );
    }

    private Analog CaptureMove(float time) {
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
      StateA state;
      StateA prevState = prevMove.State;

      if (direction.IsNeutral()) {
        if (speed < K.IdleSpeed || prevState == StateA.Inactive) {
          state = StateA.Inactive;
        } else {
          state = StateA.Unknown;
        }
      } else if (direction == prevMove.Direction) {
        if (prevState == StateA.Active || prevState == StateA.Switch || prevState == StateA.SwitchTap) {
          state = StateA.Active;
        } else if (speed < K.TapSpeed) {
          state = StateA.Switch;
        } else if (speed >= K.TapSpeed && mag == 1.0f) {
          state = StateA.SwitchTap;
        } else {
          state = StateA.Unknown;
        }
      } else {
        if (speed >= K.TapSpeed && mag == 1.0f) {
          state = StateA.SwitchTap;
        } else if (speed >= K.TapSpeed) {
          state = StateA.Unknown;
        } else {
          state = StateA.Switch;
        }
      }

      if (state == StateA.Switch || state == StateA.SwitchTap) {
        Log.Verbose($"[Input.Stream] SwitchDirection(dir: {direction}, state: {state})");
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

    // -- queries/helpers
    public static Direction DirectionFromVec(Vec v) {
      if (v.X == 0.0f && v.Y == 0.0f) {
        return Direction.Neutral;
      } else if (Mathf.Abs(v.X) > Mathf.Abs(v.Y)) {
        return v.X > 0.0f ? Direction.Right : Direction.Left;
      } else {
        return v.Y > 0.0f ? Direction.Up : Direction.Down;
      }
    }
  }
}
