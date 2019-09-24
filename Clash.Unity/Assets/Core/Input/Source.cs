using U = UnityEngine;
using C = Clash;

namespace Input {
  public sealed class Source: C.Input.ISource {
    // -- ISource --
    public float GetAxis(string name) {
      return U.Input.GetAxisRaw(name);
    }

    public bool GetButton(string name) {
      return U.Input.GetButton(name);
    }

    public bool GetButtonDown(string name) {
      return U.Input.GetButtonDown(name);
    }

    public bool GetButtonUp(string name) {
      return U.Input.GetButtonUp(name);
    }
  }
}
