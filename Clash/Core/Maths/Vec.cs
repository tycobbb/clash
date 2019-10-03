namespace Clash.Maths {
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
    public static Vec operator +(Vec a, Vec b) {
      return new Vec(
        a.X + b.X,
        a.Y + b.Y
      );
    }

    public static Vec operator -(Vec a, Vec b) {
      return new Vec(
        a.X - b.X,
        a.Y - b.Y
      );
    }

    public static Vec operator *(Vec a, float s) {
      return new Vec(
        a.X * s,
        a.Y * s
      );
    }

    public static Vec operator /(Vec a, float s) {
      return new Vec(
        a.X / s,
        a.Y / s
      );
    }

    // -- queries --
    public float Mag() {
      return Mathf.Hypot(X, Y);
    }

    public Vec Reverse() {
      return new Vec(-X, -Y);
    }

    public Vec Normalize() {
      var mag = Mag();

      if (mag == 0.0f) {
        return Zero;
      } else {
        return this / mag;
      }
    }

    // -- factories --
    public static Vec Zero = default;

    // -- debug --
    public override string ToString() {
      return $"<Vec | X={X} Y={Y}>";
    }
  }
}
