namespace Input {
  public struct Snapshot {
    public Analog mMove { get; }
    public Button mJumpA { get; }
    public Button mJumpB { get; }

    // -- lifetime --
    internal Snapshot(Analog move, Button jumpA, Button jumpB) {
      mMove = move;
      mJumpA = jumpA;
      mJumpB = jumpB;
    }
  }
}
