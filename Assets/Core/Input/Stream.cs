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
      var current = mCurrent.mMove;

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

      Analog.State state;
      if (direction != current.mDirection) {
        state = Analog.State.Switch;
      } else {
        state = Analog.State.Active;
      }

      if (state == Analog.State.Switch) {
        Log.Debug("[Input.Stream] Switch Direction: " + direction);
      }

      return new Analog(state, direction, position);
    }

    private Button CaptureButton(string name) {
      Button.State state;
      if (U.Input.GetButtonDown(name)) {
        state = Button.State.Down;
      } else if (U.Input.GetButtonUp(name)) {
        state = Button.State.Up;
      } else {
        state = U.Input.GetButton(name) ? Button.State.Active : Button.State.Inactive;
      }

      return new Button(state);
    }
  }
}
