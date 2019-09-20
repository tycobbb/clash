using U = UnityEngine;
using Input.Ext;

namespace Input {
  public struct Analog {
    public State mState { get; }
    public Direction mDirection { get; }
    public U.Vector2 mPosition { get; }

    // -- lifetime --
    internal Analog(State state, Direction direction, U.Vector2 position) {
      mState = state;
      mDirection = direction;
      mPosition = position;
    }

    // -- queries --
    public bool IsNeutral() {
      return mDirection.IsNeutral();
    }

    public bool IsLeft() {
      return mDirection.IsLeft();
    }

    public bool IsRight() {
      return mDirection.IsRight();
    }

    public bool DidSwitch() {
      return mState == State.Switch;
    }

    // -- types --
    public enum State {
      Switch,
      Active
    }
  }
}
