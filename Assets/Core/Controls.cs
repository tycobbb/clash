using UnityEngine;

public interface IControls {
  float GetMoveX();
  bool GetJump();
  bool GetJumpDown();
  bool GetJump2Down();
}

public sealed class Controls: IControls {
  public float GetMoveX() {
    return Input.GetAxis("MoveX");
  }

  public bool GetJump() {
    return Input.GetButton("Jump");
  }

  public bool GetJumpDown() {
    return Input.GetButtonDown("Jump");
  }

  public bool GetJump2Down() {
    return Input.GetButtonDown("Jump2");
  }
}
