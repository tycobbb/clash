using UnityEngine;

public sealed class Controls {
  // -- queries --
  public bool IsJumpDown() {
    return Input.GetButtonDown("Jump");
  }

  public float LeftStickX() {
    return Input.GetAxis("LeftStickX");
  }
}
