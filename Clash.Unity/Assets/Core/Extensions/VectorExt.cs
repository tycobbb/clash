using VecNative = UnityEngine.Vector2;
using VecDomain = Clash.Maths.Vec;

public static class VectorExt {
  public static VecDomain ToDomain(this VecNative v) {
    return new VecDomain(v.x, v.y);
  }

  public static VecNative ToNative(this VecDomain v) {
    return new VecNative(v.X, v.Y);
  }
}
