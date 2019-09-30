namespace Clash.Input {
  public struct Button {
    public StateB State { get; }

    // -- lifetime --
    public Button(StateB state) {
      State = state;
    }

    // -- queries --
    public bool IsDown() {
      return State == StateB.Down;
    }

    public bool IsActive() {
      return State == StateB.Down || State == StateB.Active;
    }

    // -- debug --
    public override string ToString() {
      return $"<Button | State={State}>";
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
