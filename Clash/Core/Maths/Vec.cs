namespace Clash.Maths {
  // -- impls --
  public struct Vec {
    // -- properties --
    public float X { get; set; }
    public float Y { get; set; }

    // -- lifetime --
    public Vec(float x, float y) {
      X = x;
      Y = y;
    }

    // -- operators --
    public static Vec operator -(Vec a, Vec b) {
      return new Vec(
        a.X - b.X,
        a.Y - b.Y
      );
    }

    // -- queries --
    public float Mag() {
      return Mathf.Hypot(X, Y);
    }

    // -- factories --
    public static Vec Zero = default;

    // -- debug --
    public override string ToString() {
      return $"<Vec | X={X} Y={Y}>";
    }
  }
}
