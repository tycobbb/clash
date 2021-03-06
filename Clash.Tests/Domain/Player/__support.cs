using NSubstitute;
using Clash.Maths;
using Clash.Input.Tests;

namespace Clash.Player.Tests {
  using K = Config;

  public static class Players {
    public static Player MakeIdle() {
      var player = new Player(isOnGround: true);
      player.OnPreUpdate(Vec.Zero);
      return player;
    }

    public static Player MakeDash(Input.IStream stream, float x) {
      var player = MakeIdle();

      player.Simulate(stream,
        input: Snapshots.MakeTap(x)
      );

      return player;
    }

    public static Player MakeRun(Input.IStream stream, float x) {
      var player = MakeDash(stream, x);

      player.Simulate(stream,
        input: Snapshots.MakeTilt(x),
        frame: K.DashFrames
      );

      return player;
    }

    public static Player MakePivot(Input.IStream stream, float x) {
      var player = MakeRun(stream, -x);

      player.Simulate(stream,
        input: Snapshots.MakeTilt(x)
      );

      return player;
    }

    public static Player MakeJumpWait(Input.IStream stream) {
      var player = MakeIdle();

      var frame = player.State.Frame;
      player.Simulate(stream,
        input: Snapshots.MakeJumpA(Input.StateB.Down)
      );

      player.Simulate(stream,
        frame: frame + K.WaveDashFrameWindow
      );

      return player;
    }

    public static Player MakeAirborne(Input.IStream stream) {
      var player = MakeJumpWait(stream);

      player.Simulate(stream,
        input: Snapshots.MakeJumpA(Input.StateB.Down)
      );

      player.Simulate(stream,
        input: Snapshots.MakeJumpA(),
        frame: K.JumpWaitFrames
      );

      return player;
    }

    public static Player MakeAirDodge(Input.IStream stream) {
      var player = MakeAirborne(stream);

      player.Simulate(stream,
        input: Snapshots.MakeShieldL(Input.StateB.Down)
      );

      return player;
    }

    public static Player MakeWaveDash(Input.IStream stream) {
      var player = MakeJumpWait(stream);

      player.Simulate(stream,
        input: Snapshots.MakeShieldL(Input.StateB.Down)
      );

      return player;
    }
  }

  public static class PlayerExt {
    public static void Simulate(this Player player, Input.IStream stream, Input.Snapshot? input = null, int frame = -1) {
      // simulate input or an empty one
      stream.GetCurrent().Returns(input.GetValueOrDefault());

      // set frame, if any
      if (frame != -1) {
        player.State.Frame = frame;
      }

      // simulate lifecycle with crude physics
      player.OnUpdate(stream);
      player.OnPostSimulation(player.Velocity + player.Force);
      player.OnPreUpdate(player.Velocity + player.Force);
    }
  }
}
