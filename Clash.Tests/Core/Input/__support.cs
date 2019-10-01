using Clash.Maths;

namespace Clash.Input.Tests {
  public static class Snapshots {
    /*
      TODO: turn this into a builder, and overload Player.Simulate (Player/__support.cs)
      to also accept an ISnapshotProducer (() => Snapshot).

      player.Simulate(stream,
        input: Snapshots.builder()
          .move(Input.StateA.Active, x: -1.0f)
          .shieldL(Input.StateB.Down)
        )
      );
    */
    public static Snapshot Make(
      Analog move = default,
      Button jumpA = default,
      Button jumpB = default,
      Button shieldL = default,
      Button shieldR = default,
      float time = default
    ) {
      return new Snapshot(
        move,
        jumpA,
        jumpB,
        shieldL,
        shieldR,
        time
      );
    }

    // -- factories --
    public static Snapshot MakeTap(
      float x = default,
      float y = default
    ) {
      return MakeMove(StateA.SwitchTap, x, y);
    }

    public static Snapshot MakeTilt(
      float x = default,
      float y = default
    ) {
      return MakeMove(StateA.Active, x, y);
    }

    public static Snapshot MakeMove(
      StateA state = default,
      float x = default,
      float y = default
    ) {
      return Make(
        move: MakeAnalog(state, x, y)
      );
    }

    public static Snapshot MakeJumpA(
      StateB state = StateB.Active
    ) {
      return Make(
        jumpA: new Button(state)
      );
    }

    public static Snapshot MakeJumpB(
      StateB state = StateB.Active
    ) {
      return Make(
        jumpB: new Button(state)
      );
    }

    public static Snapshot MakeShieldL(
      StateB state = StateB.Active
    ) {
      return Make(
        shieldL: new Button(state)
      );
    }

    // -- factories/components
    public static Analog MakeAnalog(
      StateA state = default,
      float x = default,
      float y = default
    ) {
      var pos = new Vec(x, y);

      // determine direction
      // TODO: any way to not copy this from Input.Stream?
      Direction direction;
      if (pos.X == 0.0f && pos.Y == 0.0f) {
        direction = Direction.Neutral;
      } else if (Mathf.Abs(pos.X) > Mathf.Abs(pos.Y)) {
        direction = pos.X > 0.0f ? Direction.Right : Direction.Left;
      } else {
        direction = pos.Y > 0.0f ? Direction.Up : Direction.Down;
      }

      return new Analog(state, direction, pos, pos);
    }

    public static Button MakeButton(StateB button = default) {
      return new Button(button);
    }
  }
}
