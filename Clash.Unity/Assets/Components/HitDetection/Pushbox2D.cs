using UnityEngine;
using C = Clash;

public class Pushbox2D: MonoBehaviour {
  // -- constants --
  private static readonly Color Color = new Color(1.0f, 0.0f, 1.0f, 0.5f);

  // -- lifecycle --
  public void Awake() {
    gameObject.AddComponent<BoxCollider2D>();
  }

  public void OnDrawGizmos() {
    var collider = Collider();
    if (collider == null) {
      return;
    }

    Gizmos.color = Color;
    Gizmos.matrix = transform.localToWorldMatrix;
    Gizmos.DrawCube(collider.offset, collider.size);
  }

  // -- commands --
  public void SetFrame(Vector2 offset, Vector2 size) {
    var collider = Collider();
    collider.offset = offset;
    collider.size = size;
  }

  // -- queries --
  private BoxCollider2D Collider() {
    return GetComponent<BoxCollider2D>();
  }
}
