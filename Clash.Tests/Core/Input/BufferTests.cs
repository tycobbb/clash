using System;
using NUnit.Framework;
using NSubstitute;
using Clash.Maths;

namespace Clash.Input.Tests {
  // -- tests --
  public class BufferTests {
    [Test]
    public void ItReturnsTheDefaultValueForAnUnusedOffset() {
      var buffer = new Buffer(size: 1);
      Assert.That(buffer[0].JumpA.State, Is.EqualTo(StateB.Inactive));
    }

    [Test]
    public void ItThrowsForAnOutOfRangeOffset() {
      var buffer = new Buffer(size: 1);
      buffer.Add(Snapshots.MakeJumpA(StateB.Active));
      Assert.That(() => buffer[1], Throws.Exception.TypeOf<IndexOutOfRangeException>());
    }

    [Test]
    public void ItReturnsTheNewestSnapshotFirst() {
      var buffer = new Buffer(size: 2);
      buffer.Add(Snapshots.MakeJumpA(StateB.Active));
      buffer.Add(Snapshots.MakeJumpB(StateB.Active));

      Assert.That(buffer[0].JumpB.State, Is.EqualTo(StateB.Active));
      Assert.That(buffer[1].JumpA.State, Is.EqualTo(StateB.Active));
    }

    [Test]
    public void ItRemovesTheOldestSnapshotWhenAtCapacity() {
      var buffer = new Buffer(size: 3);
      buffer.Add(Snapshots.MakeJumpA(StateB.Active));
      buffer.Add(Snapshots.MakeShieldL(StateB.Active));
      buffer.Add(Snapshots.MakeJumpB(StateB.Active));
      buffer.Add(Snapshots.MakeShieldR(StateB.Active));

      Assert.That(buffer[0].ShieldR.State, Is.EqualTo(StateB.Active));
      Assert.That(buffer[1].JumpB.State, Is.EqualTo(StateB.Active));
      Assert.That(buffer[2].ShieldL.State, Is.EqualTo(StateB.Active));
      Assert.That(() => buffer[3], Throws.Exception.TypeOf<IndexOutOfRangeException>());
    }
  }
}
