namespace Input {
  public struct Snapshot {
    public Analog Move { get; }
    public Button JumpA { get; }
    public Button JumpB { get; }

    // -- lifetime --
    internal Snapshot(Analog move, Button jumpA, Button jumpB) {
      Move = move;
      JumpA = jumpA;
      JumpB = jumpB;
    }

    // -- debug --
    public override string ToString() {
      return $"<Snapshot | Move={Move} JumpA={JumpA} JumpB={JumpB}>";
    }
  }
}
