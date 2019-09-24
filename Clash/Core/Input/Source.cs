namespace Clash.Input {
  // -- interface
  public interface ISource {
    float GetAxis(string name);
    bool GetButtonDown(string name);
    bool GetButton(string name);
    bool GetButtonUp(string name);
  }
}
