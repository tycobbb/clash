namespace Player {
  // -- names --
  internal enum StateName {
    Idle = 1 << 0,
    Walk = 1 << 1,
    Dash = 1 << 2,
    Run = 1 << 3,
    JumpWait = 1 << 4,
    Airborne = 1 << 5
  }

  // -- states --
  internal sealed class Idle: State {
    internal Idle() : base(StateName.Idle) { }
  }

  internal sealed class Walk: State {
    internal Walk() : base(StateName.Walk) { }
  }

  internal sealed class Dash: State {
    internal readonly Input.Direction Direction;

    internal Dash(Input.Direction direction) : base(StateName.Dash) {
      Direction = direction;
    }
  }

  internal sealed class Run: State {
    internal readonly Input.Direction Direction;

    internal Run(Input.Direction direction) : base(StateName.Run) {
      Direction = direction;
    }
  }

  internal sealed class JumpWait: State {
    internal bool IsShort;

    internal JumpWait(bool isShort) : base(StateName.JumpWait) {
      IsShort = isShort;
    }
  }

  internal sealed class Airborne: State {
    internal bool IsFalling;

    internal Airborne(bool isFalling) : base(StateName.Airborne) {
      IsFalling = isFalling;
    }
  }
}
