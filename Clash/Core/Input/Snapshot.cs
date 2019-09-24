namespace Clash.Input {
  public struct Snapshot {
    public Analog Move { get; }
    public Button JumpA { get; }
    public Button JumpB { get; }
    public float Time { get; }

    // -- lifetime --
    internal Snapshot(Analog move, Button jumpA, Button jumpB, float time) {
      Move = move;
      JumpA = jumpA;
      JumpB = jumpB;
      Time = time;
    }

    // -- debug --
    public override string ToString() {
      return $"<Snapshot | Move={Move} JumpA={JumpA} JumpB={JumpB}>";
    }
  }
}
