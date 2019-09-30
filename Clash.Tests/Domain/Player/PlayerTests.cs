using NUnit.Framework;
using NSubstitute;
using Clash.Maths;
using Clash.Input.Tests;

namespace Clash.Player.Tests {
  using K = Config;

  // -- tests --
  public class PlayerTests {
    [Test]
    public void ItStartsFacingRight() {
      var player = new Player(isOnGround: true);
      Assert.That(player.State, Is.InstanceOf<Idle>());
      Assert.That(player.IsFacingLeft, Is.False, "Should be facing right, but was facing left.");
    }

    // -- tests/move
    [Test]
    public void ItWalksWhenTiltingADirection() {
      var player = Players.MakeIdle();
      var stream = Substitute.For<Input.IStream>();

      player.SimulateInput(stream,
        input: Snapshots.MakeTilt(Input.Direction.Left, x: -0.5f)
      );

      Assert.That(player.State, Is.InstanceOf<Walk>());
      Assert.That(player.IsFacingLeft, Is.True, "Should be facing left, but was facing right.");
    }

    [Test]
    public void ItDashesWhenTappingADirection() {
      var player = Players.MakeIdle();
      var stream = Substitute.For<Input.IStream>();

      player.SimulateInput(stream,
        input: Snapshots.MakeTap(Input.Direction.Left)
      );

      Assert.That(player.State, Is.InstanceOf<Dash>());
      Assert.That(player.IsFacingLeft, Is.True, "Should be facing left, but was facing right.");
    }

    [Test]
    public void ItDashesBackWhenTappingTheOppositeDirection() {
      var stream = Substitute.For<Input.IStream>();
      var player = Players.MakeDash(stream, Input.Direction.Left);

      player.SimulateInput(stream,
        input: Snapshots.MakeTap(Input.Direction.Right)
      );

      Assert.That(player.State, Is.InstanceOf<Dash>());
      Assert.That(player.State.Frame, Is.EqualTo(1));
      Assert.That(player.IsFacingLeft, Is.False, "Should be facing right, but was facing left.");
    }

    [Test]
    public void ItPivotsWhenChangingRunDirection() {
      var stream = Substitute.For<Input.IStream>();
      var player = Players.MakeRun(stream, Input.Direction.Left);

      player.SimulateInput(stream,
        input: Snapshots.MakeTap(Input.Direction.Right)
      );

      Assert.That(player.State, Is.InstanceOf<Pivot>());
      Assert.That(player.IsFacingLeft, Is.False, "Should be facing right, but was facing left.");
    }

    // -- tests/jump
    [Test]
    public void ItJumpsWhenPressingTheJumpButton() {
      var player = Players.MakeIdle();
      var stream = Substitute.For<Input.IStream>();

      player.SimulateInput(stream,
        input: Snapshots.MakeJumpA(Input.StateB.Down)
      );

      Assert.That(player.State, Is.InstanceOf<JumpWait>());
    }

    [Test]
    public void ItFullJumpsWhenTheButtonIsDownUntilTakeoff() {
      var stream = Substitute.For<Input.IStream>();
      var player = Players.MakeJumpWait(stream);

      player.SimulateInput(stream,
        input: Snapshots.MakeJumpA(),
        frame: K.JumpWaitFrames
      );

      Assert.That(player.State, Is.InstanceOf<Airborne>());
      Assert.That(player.Velocity.Y, Is.EqualTo(K.Jump));
    }

    [Test]
    public void ItShortJumpsWhenTheButtonIsUpBeforeTakeoff() {
      var stream = Substitute.For<Input.IStream>();
      var player = Players.MakeJumpWait(stream);

      player.SimulateInput(stream,
        input: Snapshots.MakeJumpA(Input.StateB.Up),
        frame: K.JumpWaitFrames
      );

      Assert.That(player.State, Is.InstanceOf<Airborne>());
      Assert.That(player.Velocity.Y, Is.EqualTo(K.JumpShort));
    }

    [Test]
    public void ItShortJumpsWhenPressingTheShortJumpButton() {
      var player = Players.MakeIdle();
      var stream = Substitute.For<Input.IStream>();

      player.SimulateInput(stream,
        input: Snapshots.MakeJumpB(Input.StateB.Down)
      );

      player.SimulateInput(stream,
        input: Snapshots.MakeJumpB(),
        frame: K.JumpWaitFrames
      );

      Assert.That(player.State, Is.InstanceOf<Airborne>());
      Assert.That(player.Velocity.Y, Is.EqualTo(K.JumpShort));
    }
  }
}
