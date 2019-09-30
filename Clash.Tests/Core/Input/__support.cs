using Clash.Maths;

namespace Clash.Input.Tests {
  public static class Snapshots {
    public static Snapshot MakeTap(
      Direction direction = default,
      float x = default,
      float y = default
    ) {
      return MakeMove(StateA.SwitchTap, direction, x, y);
    }

    public static Snapshot MakeTilt(
      Direction direction = default,
      float x = default,
      float y = default
    ) {
      return MakeMove(StateA.Active, direction, x, y);
    }

    public static Snapshot MakeMove(
      StateA state = default,
      Direction direction = default,
      float x = default,
      float y = default
    ) {
      var position = new Vec(x, y);
      var snapshot = new Snapshot(
        move: new Analog(state, direction, position, position),
        jumpA: default,
        jumpB: default,
        shieldL: default,
        shieldR: default,
        time: default
      );

      return snapshot;
    }

    public static Snapshot MakeJumpA(
      StateB state = StateB.Active
    ) {
      var snapshot = new Snapshot(
        move: default,
        jumpA: new Button(state),
        jumpB: default,
        shieldL: default,
        shieldR: default,
        time: default
      );

      return snapshot;
    }

    public static Snapshot MakeJumpB(
      StateB state = StateB.Active
    ) {
      var snapshot = new Snapshot(
        move: default,
        jumpA: default,
        jumpB: new Button(state),
        shieldL: default,
        shieldR: default,
        time: default
      );

      return snapshot;
    }
  }
}
