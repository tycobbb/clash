namespace Clash.Player {
  public abstract class State {
    // -- properties
    public int Frame;

    // -- lifetime
    public State() {
      Frame = 0;
    }

    // -- commands
    public void AdvanceFrame() {
      Frame++;
    }
  }
}
