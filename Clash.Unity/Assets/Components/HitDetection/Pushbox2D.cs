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
    Gizmos.DrawCube(Vector3.zero, collider.size);
  }

  // -- commands --
  public void SetSize(Vector2 size) {
    Collider().size = size;
  }

  // -- queries --
  private BoxCollider2D Collider() {
    return GetComponent<BoxCollider2D>();
  }
}
