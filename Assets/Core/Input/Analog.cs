using U = UnityEngine;

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
      return mDirection == Direction.Neutral;
    }

    public bool IsDown() {
      return mDirection == Direction.Down;
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
