namespace Input {
  public struct Button {
    public StateB State { get; }

    // -- lifetime --
    internal Button(StateB state) {
      State = state;
    }

    // -- queries --
    public bool IsDown() {
      return State == StateB.Down;
    }

    public bool IsActive() {
      return State == StateB.Down || State == StateB.Active;
    }
  }

  // -- types --
  public enum StateB {
    Inactive,
    Down,
    Active,
    Up
  }
}
