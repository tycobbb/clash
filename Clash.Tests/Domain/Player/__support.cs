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

    public static Player MakeDash(Input.IStream stream, Input.Direction direction) {
      var player = MakeIdle();
      player.SimulateInput(stream, Snapshots.MakeTap(direction));
      return player;
    }

    public static Player MakeRun(Input.IStream stream, Input.Direction direction) {
      var player = MakeDash(stream, direction);
      player.SimulateInput(stream, Snapshots.MakeTilt(direction), frame: K.DashFrames);
      return player;
    }

    public static Player MakeJumpWait(Input.IStream stream) {
      var player = MakeIdle();
      player.SimulateInput(stream, Snapshots.MakeJumpA(Input.StateB.Down));
      return player;
    }
  }

  public static class PlayerExt {
    public static void Simulate(this Player player, Input.IStream stream) {
      player.OnUpdate(stream);
      player.OnPostSimulation(player.Velocity);
      player.OnPreUpdate(player.Velocity);
    }

    public static void SimulateInput(this Player player, Input.IStream stream, Input.Snapshot input, int frame = -1) {
      if (frame != -1) {
        player.State.Frame = frame;
      }

      stream.GetCurrent().Returns(input);
      player.Simulate(stream);
    }
  }
}
