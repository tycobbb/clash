using System;
using NUnit.Framework;
using NSubstitute;
using Clash.Maths;

namespace Clash.Input.Tests {
  // -- tests --
  public class GestureTests {
    [Test]
    public void ItBeginsInAnUnknownState() {
      var r = Substitute.For<IGestureRecognizer>();
      var gesture = new Gesture(r);
      Assert.That(gesture.State, Is.EqualTo(StateG.Possible));
    }

    [Test]
    public void ItUpdatesToTheRecognizersState() {
      var r = Substitute.For<IGestureRecognizer>();
      var gesture = new Gesture(r);

      var s = Substitute.For<IStream>();
      r.OnInput(gesture, s).Returns(StateG.Pending);

      gesture.OnUpdate(s);
      Assert.That(gesture.State, Is.EqualTo(StateG.Pending));
    }

    [Test]
    public void ItRecogizesOnceItIsSatisfied() {
      var r = Substitute.For<IGestureRecognizer>();
      var gesture = new Gesture(r);

      var s = Substitute.For<IStream>();
      r.OnInput(gesture, s).Returns(StateG.Satisfied);

      gesture.OnUpdate(s);
      Assert.That(gesture.IsRecognized, Is.True);
    }

    [Test]
    public void ItDoesNotRecognizeWhenThePreviousGestureIs(
      [Values(StateG.Pending, StateG.Satisfied)] StateG state
    ) {
      var r1 = Substitute.For<IGestureRecognizer>();
      var r2 = Substitute.For<IGestureRecognizer>();
      var gesture1 = new Gesture(r1);
      var gesture2 = new Gesture(r2);

      var s = Substitute.For<IStream>();
      r1.OnInput(gesture1, s).Returns(state);
      r2.OnInput(gesture2, s).Returns(StateG.Satisfied);

      gesture1.AddNext(gesture2);
      gesture1.OnUpdate(s);

      Assert.That(gesture2.IsRecognized, Is.False);
    }

    [Test]
    public void ItRecognizesWhenThePreviousGestureFails() {
      var r1 = Substitute.For<IGestureRecognizer>();
      var r2 = Substitute.For<IGestureRecognizer>();
      var gesture1 = new Gesture(r1);
      var gesture2 = new Gesture(r2);

      var s = Substitute.For<IStream>();
      r1.OnInput(gesture1, s).Returns(StateG.Pending);
      r2.OnInput(gesture2, s).Returns(StateG.Satisfied);

      gesture1.AddNext(gesture2);
      gesture1.OnUpdate(s);

      r1.OnInput(gesture1, s).Returns(StateG.Failed);
      gesture1.OnUpdate(s);

      Assert.That(gesture2.IsRecognized, Is.True);
    }
  }
}
