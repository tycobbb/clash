namespace Player {
  // -- names --
  public enum StateName {
    Idle = 1 << 0,
    Walk = 1 << 1,
    Dash = 1 << 2,
    Run = 1 << 3,
    JumpWait = 1 << 4,
    Airborne = 1 << 5
  }

  // -- states --
  public sealed class Idle: State {
    public Idle() : base(StateName.Idle) { }
  }

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
}
