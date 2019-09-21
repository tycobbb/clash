using U = UnityEngine;

namespace Input {
  // -- interfaces --
  public interface IStream {
    Snapshot GetCurrent();
  }

  public interface IMutableStream: IStream {
    void OnUpdate();
  }

  // -- impls --
  public sealed class Stream: IMutableStream {
    private Snapshot mCurrent;

    // -- constants --
    private const float kDeadZone = 0.1f;

    // -- IStream --
    public Snapshot GetCurrent() {
      return mCurrent;
    }

    // -- IMutableStream --
    public void OnUpdate() {
      mCurrent = CaptureSnapshot();
    }

    // -- queries --
    private Snapshot CaptureSnapshot() {
      return new Snapshot(
        CaptureMove(),
        CaptureButton("JumpA"),
        CaptureButton("JumpB")
      );
    }

    private Analog CaptureMove() {
      var current = mCurrent.Move;

      var position = new U.Vector2(
        U.Input.GetAxis("MoveX"),
        U.Input.GetAxis("MoveY")
      );

      if (U.Mathf.Abs(position.x) <= kDeadZone) {
        position.x = 0.0f;
      }

      if (U.Mathf.Abs(position.y) <= kDeadZone) {
        position.y = 0.0f;
      }

      Direction direction;
      if (position.x == 0.0f && position.y == 0.0f) {
        direction = Direction.Neutral;
      } else if (U.Mathf.Abs(position.x) > U.Mathf.Abs(position.y)) {
        direction = position.x > 0.0f ? Direction.Right : Direction.Left;
      } else {
        direction = position.y > 0.0f ? Direction.Up : Direction.Down;
      }

      StateA state;
      if (direction != current.Direction) {
        state = StateA.Switch;
      } else {
        state = StateA.Active;
      }

      if (state == StateA.Switch) {
        Log.Verbose("[Input.Stream] Switch Direction: " + direction);
      }

      return new Analog(state, direction, position);
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
  }
}
