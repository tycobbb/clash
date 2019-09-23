using U = UnityEngine;

namespace Input {
  using Ext;
  using K = Config;

  // -- interfaces --
  public interface IStream {
    Snapshot GetCurrent();
  }

  public interface IMutableStream: IStream {
    void OnUpdate();
  }

  // -- impls --
  public sealed class Stream: IMutableStream {
    private Snapshot current;

    // -- IStream --
    public Snapshot GetCurrent() {
      return current;
    }

    // -- IMutableStream --
    public void OnUpdate() {
      current = CaptureSnapshot(U.Time.fixedUnscaledTime);
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
      var pos = new U.Vector2(
        U.Input.GetAxisRaw("MoveX"),
        U.Input.GetAxisRaw("MoveY")
      );

      // filter raw position
      if (pos.magnitude <= K.DeadZone) {
        pos = U.Vector2.zero;
      }

      // determine primary direction
      Direction direction;
      if (pos.x == 0.0f && pos.y == 0.0f) {
        direction = Direction.Neutral;
      } else if (U.Mathf.Abs(pos.x) > U.Mathf.Abs(pos.y)) {
        direction = pos.x > 0.0f ? Direction.Right : Direction.Left;
      } else {
        direction = pos.y > 0.0f ? Direction.Up : Direction.Down;
      }

      // calc instantaneous stick speed
      var delta = pos - prevMove.Position;
      var speed = U.Mathf.Abs(delta.magnitude / (time - prevTime));

      // calc the magnitude of the primary direction
      var mag = U.Mathf.Abs(direction.IsHorizontal() ? pos.x : pos.y);

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

      return new Analog(state, direction, pos);
    }

    private Button CaptureButton(string name) {
      StateB state;
      if (U.Input.GetButtonDown(name)) {
        state = StateB.Down;
      } else if (U.Input.GetButtonUp(name)) {
        state = StateB.Up;
      } else {
        state = U.Input.GetButton(name) ? StateB.Active : StateB.Inactive;
      }

      return new Button(state);
    }

    // -- queries/helpers
    public static Direction DirectionFromVec(U.Vector2 vec) {
      if (vec.x == 0.0f && vec.y == 0.0f) {
        return Direction.Neutral;
      } else if (U.Mathf.Abs(vec.x) > U.Mathf.Abs(vec.y)) {
        return vec.x > 0.0f ? Direction.Right : Direction.Left;
      } else {
        return vec.y > 0.0f ? Direction.Up : Direction.Down;
      }
    }
  }
}
