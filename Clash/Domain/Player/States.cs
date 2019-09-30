using Clash.Maths;

namespace Clash.Player {
  // -- names --
  public enum StateName {
    Idle,
    Walk,
    Dash,
    Run,
    Pivot,
    Skid,
    JumpWait,
    Airborne,
    AirDodge
  }

  // -- states --
  public sealed class Idle: State {
    public Idle() : base(StateName.Idle) { }
  }

  // -- states/move
  public sealed class Walk: State {
    public Walk() : base(StateName.Walk) { }
  }

  public sealed class Dash: State {
    public readonly Input.Direction Direction;

    public Dash(Input.Direction direction) : base(StateName.Dash) {
      Direction = direction;
    }
  }

  public sealed class Run: State {
    public readonly Input.Direction Direction;

    public Run(Input.Direction direction) : base(StateName.Run) {
      Direction = direction;
    }
  }

  public sealed class Pivot: State {
    public Pivot() : base(StateName.Pivot) { }
  }

  public sealed class Skid: State {
    public Skid() : base(StateName.Skid) { }
  }

  // -- states/jump
  public sealed class JumpWait: State {
    public bool IsShort;

    public JumpWait(bool isShort) : base(StateName.JumpWait) {
      IsShort = isShort;
    }
  }

  public sealed class Airborne: State {
    public bool IsFalling;

    public Airborne(bool isFalling) : base(StateName.Airborne) {
      IsFalling = isFalling;
    }
  }

  public sealed class AirDodge: State {
    public Vec Direction;
    public bool IsOnGround;

    public AirDodge(Vec direction, bool isOnGround = true) : base(StateName.AirDodge) {
      Direction = direction;
      IsOnGround = isOnGround;
    }
  }
}
