namespace Clash.Input {
  public enum Direction {
    Neutral = 1 << 0,
    Up = 1 << 1,
    Down = 1 << 2,
    Left = 1 << 3,
    Right = 1 << 4,
    Vertical = Up | Down,
    Horizontal = Left | Right,
  }

  // -- extensions --
  namespace Ext {
    public static class DirectionExt {
      // -- queries --
      public static Direction Invert(this Direction direction) {
        switch (direction) {
          case Direction.Up:
            return Direction.Down;
          case Direction.Down:
            return Direction.Up;
          case Direction.Left:
            return Direction.Right;
          case Direction.Right:
            return Direction.Left;
          default:
            return direction;
        }
      }

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
