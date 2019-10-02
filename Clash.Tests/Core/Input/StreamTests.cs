using NUnit.Framework;
using NSubstitute;
using Clash.Maths;

namespace Clash.Input.Tests {
  // -- tests --
  public class StreamTests {
    [Test]
    public void ItFiltersTheStickDeadZone() {
      var source = Substitute.For<ISource>();
      var stream = new Stream(source);

      source.GetAxis("MoveX").Returns(0.15f);
      source.GetAxis("MoveY").Returns(0.10f);

      stream.OnUpdate(0.0f);
      var move = stream.GetCurrent().Move;
      Assert.That(move.Position, Is.EqualTo(Vec.Zero));
      Assert.That(move.RawPosition, Is.EqualTo(new Vec(0.15f, 0.10f)));
    }

    [Test]
    public void ItRemainsInactiveWhileTheStickIsNeutral() {
      var source = Substitute.For<ISource>();
      var stream = new Stream(source, Snapshots.MakeMove(StateA.Inactive));

      stream.OnUpdate(1.0f);
      var move = stream.GetCurrent().Move;
      Assert.That(move.State, Is.EqualTo(StateA.Inactive));
    }

    [Test]
    public void ItBecomesInactiveIfTheStickIsNeutralAndIdle() {
      var source = Substitute.For<ISource>();
      var stream = new Stream(source, Snapshots.MakeMove(StateA.Active, x: 0.0f));
      source.GetAxis("MoveX").Returns(0.19f);

      stream.OnUpdate(1.0f);
      var move = stream.GetCurrent().Move;
      Assert.That(move.State, Is.EqualTo(StateA.Inactive));
    }

    [Test]
    public void ItBecomesUnknownIfTheStickIsNeutralButNotIdle() {
      var source = Substitute.For<ISource>();
      var stream = new Stream(source, Snapshots.MakeMove(StateA.Active, x: 1.0f));
      source.GetAxis("MoveX").Returns(0.0f);

      stream.OnUpdate(1.0f);
      var move = stream.GetCurrent().Move;
      Assert.That(move.State, Is.EqualTo(StateA.Unknown));
    }

    public void ItRemainsActiveWhileTheStickIsInTheSameDirection() {
      var source = Substitute.For<ISource>();
      var stream = new Stream(source, Snapshots.MakeMove(StateA.Active, x: 1.0f));
      source.GetAxis("MoveX").Returns(1.0f);

      stream.OnUpdate(1.0f);
      var move = stream.GetCurrent().Move;
      Assert.That(move.State, Is.EqualTo(StateA.Active));
    }

    [Test]
    public void ItBecomesATapIfTheStickHitsTheEdgeOfTheStickboxQuickly(
      [Values(StateA.Inactive, StateA.Unknown)] StateA initial
    ) {
      var source = Substitute.For<ISource>();
      var stream = new Stream(source, Snapshots.MakeMove(initial, x: 0.1f));
      source.GetAxis("MoveX").Returns(1.0f);

      stream.OnUpdate(0.5f);
      var move = stream.GetCurrent().Move;
      Assert.That(move.State, Is.EqualTo(StateA.SwitchTap));
    }

    [Test]
    public void ItBecomesUnknownIfTheStickIsMovingQuicklyButHasntHitTheEdge(
      [Values(StateA.Inactive, StateA.Unknown)] StateA initial
    ) {
      var source = Substitute.For<ISource>();
      var stream = new Stream(source, Snapshots.MakeMove(initial, x: 0.1f));
      source.GetAxis("MoveX").Returns(0.9f);

      stream.OnUpdate(0.5f);
      var move = stream.GetCurrent().Move;
      Assert.That(move.State, Is.EqualTo(StateA.Unknown));
    }

    public void ItBecomesASwitchIfTheStickIsMovingSlowly(
      [Values(StateA.Inactive, StateA.Unknown)] StateA initial
    ) {
      var source = Substitute.For<ISource>();
      var stream = new Stream(source, Snapshots.MakeMove(initial, x: 0.1f));
      source.GetAxis("MoveX").Returns(0.5f);

      stream.OnUpdate(0.5f);
      var move = stream.GetCurrent().Move;
      Assert.That(move.State, Is.EqualTo(StateA.SwitchTap));
    }

    [Test]
    public void ItBecomesActiveAfterASwitchIfTheStickIsInTheSamedirection(
      [Values(StateA.Switch, StateA.SwitchTap)] StateA initial
    ) {
      var source = Substitute.For<ISource>();
      var stream = new Stream(source, Snapshots.MakeMove(initial, x: 1.0f));
      source.GetAxis("MoveX").Returns(1.0f);

      stream.OnUpdate(1.0f);
      var move = stream.GetCurrent().Move;
      Assert.That(move.State, Is.EqualTo(StateA.Active));
    }

    [Test]
    public void ItBecomesATapIfTheStickHitsTheEdgeOfTheStickboxSwitchingQuickly() {
      var source = Substitute.For<ISource>();
      var stream = new Stream(source, Snapshots.MakeMove(StateA.Active, x: -1.0f));
      source.GetAxis("MoveX").Returns(1.0f);

      stream.OnUpdate(1.0f);
      var move = stream.GetCurrent().Move;
      Assert.That(move.State, Is.EqualTo(StateA.SwitchTap));
    }

    [Test]
    public void ItBecomesUnknownIfTheStickIsSwitchingQuicklyButHasntHitTheEdge() {
      var source = Substitute.For<ISource>();
      var stream = new Stream(source, Snapshots.MakeMove(StateA.Active, x: -1.0f));
      source.GetAxis("MoveX").Returns(0.9f);

      stream.OnUpdate(1.0f);
      var move = stream.GetCurrent().Move;
      Assert.That(move.State, Is.EqualTo(StateA.Unknown));
    }

    [Test]
    public void ItBecomesASwitchIfTheStickIsSwitchingSlowly() {
      var source = Substitute.For<ISource>();
      var stream = new Stream(source, Snapshots.MakeMove(StateA.Active, x: 0.3f));
      source.GetAxis("MoveX").Returns(-0.3f);

      stream.OnUpdate(1.0f);
      var move = stream.GetCurrent().Move;
      Assert.That(move.State, Is.EqualTo(StateA.Switch));
    }
  }
}
