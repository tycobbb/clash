using UnityEngine;

public class Player: MonoBehaviour {
  // -- physics --
  private readonly Vector2 mGravity = new Vector2(0, 1);

  // -- lifecycle --
  void FixedUpdate() {
    Body().AddForce(mGravity);
  }

  // -- queries --
  private Rigidbody2D Body() {
    var body = GetComponent<Rigidbody2D>();
    if (body == null) {
      Debug.LogError("[Player] missing body!");
    }

    return body;
  }
}
