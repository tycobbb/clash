namespace Input {
  public enum Direction {
    Neutral = 1 << 0,
    Up = 1 << 1,
    Down = 1 << 2,
    Left = 1 << 3,
    Right = 1 << 4,
    Vertical = Up | Down,
    Horizontal = Left | Right,
  }

  namespace Ext {
    public static class DirectionExt {
      // -- queries --
      public static bool IsNeutral(this Direction direction) {
        return direction == Direction.Neutral;
      }

      public static bool IsLeft(this Direction direction) {
        return direction == Direction.Left;
      }

      public static bool IsRight(this Direction direction) {
        return direction == Direction.Right;
      }

      public static bool IsHorizontal(this Direction direction) {
        return direction.Intersects(Direction.Horizontal);
      }

      public static bool Intersects(this Direction direction, Direction other) {
        return (direction & other) != 0;
      }
    }
  }
}
