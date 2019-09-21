using U = UnityEngine;
using Input.Ext;

namespace Input {
  public struct Analog {
    public StateA State { get; }
    public Direction Direction { get; }
    public U.Vector2 Position { get; }

    // -- lifetime --
    internal Analog(StateA state, Direction direction, U.Vector2 position) {
      State = state;
      Direction = direction;
      Position = position;
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

    public bool DidSwitch() {
      return State == StateA.Switch;
    }
  }

  // -- types --
  public enum StateA {
    Switch,
    Active
  }
}
