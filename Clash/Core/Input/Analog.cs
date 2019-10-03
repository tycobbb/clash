using Clash.Maths;

namespace Clash.Input {
  using Ext;

  public struct Analog {
    public StateA State { get; }
    public Direction Direction { get; }
    public Vec Position { get; }
    public Vec RawPosition { get; }

    // -- lifetime --
    public Analog(
      StateA state,
      Direction direction,
      Vec position,
      Vec rawPosition
    ) {
      State = state;
      Direction = direction;
      Position = position;
      RawPosition = rawPosition;
    }

    // -- queries --
    public bool IsNeutral() {
      return Direction.IsNeutral();
    }

    public bool IsLeft() {
      return Direction.IsLeft();
    }

    public bool IsRight() {
      return Direction.IsRight();
    }

    public bool DidTap() {
      return State == StateA.SwitchTap;
    }

    // -- debug --
    public override string ToString() {
      return $"<Analog | State={State} Dir={Direction} Pos={Position}>";
    }
  }

  // -- types --
  public enum StateA {
    Idle,
    Switch,
    SwitchTap,
    Active,
    Unknown
  }
}
