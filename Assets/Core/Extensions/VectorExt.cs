using UnityEngine;

public static class VectorExt {
  public static void SetX(this Vector2 v, float x) {
    v.Set(x, v.y);
  }

  public static void SetY(this Vector2 v, float y) {
    v.Set(v.x, y);
  }

  public static Vector2 WithX(this Vector2 v, float x) {
    return new Vector2(x, v.y);
  }

  public static Vector2 WithY(this Vector2 v, float y) {
    return new Vector2(v.x, y);
  }
}
