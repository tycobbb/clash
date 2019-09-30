namespace Clash.Input {
  public struct Snapshot {
    public Analog Move { get; }
    public Button JumpA { get; }
    public Button JumpB { get; }
    public Button ShieldL { get; }
    public Button ShieldR { get; }
    public float Time { get; }

    // -- lifetime --
    public Snapshot(
      Analog move,
      Button jumpA,
      Button jumpB,
      Button shieldL,
      Button shieldR,
      float time
    ) {
      Move = move;
      JumpA = jumpA;
      JumpB = jumpB;
      ShieldL = shieldL;
      ShieldR = shieldR;
      Time = time;
    }

    // -- debug --
    public override string ToString() {
      return $"<Snapshot | Move={Move} JumpA={JumpA} JumpB={JumpB} ShieldL={ShieldL} ShieldL={ShieldL}>";
    }
  }
}
