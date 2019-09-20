namespace Input {
  public struct Button {
    public State mState { get; }

    // -- lifetime --
    internal Button(State state) {
      mState = state;
    }

    // -- queries --
    public bool IsDown() {
      return mState == State.Down;
    }

    public bool IsActive() {
      return mState == State.Down || mState == State.Active;
    }

    // -- types --
    public enum State {
      Inactive,
      Down,
      Active,
      Up
    }
  }
}
