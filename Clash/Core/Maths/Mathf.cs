using System;

namespace Clash.Maths {
  public static class Mathf {
    public static Func<float, float> Abs = Math.Abs;

    public static float Hypot(float x, float y) {
      return (float)Math.Sqrt(x * x + y * y);
    }

    public static float Clamp(float val, float min, float max) {
      return Math.Min(Math.Max(val, min), max);
    }
  }
}
