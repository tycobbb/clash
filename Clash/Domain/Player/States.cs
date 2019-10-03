using Clash.Maths;

namespace Clash.Player {
  // -- states --
  public sealed class Idle: State {
  }

  // -- states/move
  public sealed class Walk: State {
  }

  public sealed class Dash: State {
    public readonly Input.Direction Direction;

    public Dash(Input.Direction direction) {
      Direction = direction;
    }
  }

  public sealed class Run: State {
    public readonly Input.Direction Direction;

    public Run(Input.Direction direction) {
      Direction = direction;
    }
  }

  public sealed class Pivot: State {
    public readonly Input.Direction Direction;

    public Pivot(Input.Direction direction) {
      Direction = direction;
    }
  }

  public sealed class Skid: State {
  }

  // -- states/jump
  public sealed class JumpWait: State {
    public bool IsShort;

    public JumpWait(bool isShort) {
      IsShort = isShort;
    }
  }

  public sealed class Airborne: State {
    public bool IsFalling;

    public Airborne(bool isFalling) {
      IsFalling = isFalling;
    }
  }

  public sealed class AirDodge: State {
    public Vec Direction;

    public AirDodge(Vec direction) {
      Direction = direction;
    }
  }

  public sealed class WaveLand: State {
  }
}
