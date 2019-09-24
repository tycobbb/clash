using NUnit.Framework;
using NSubstitute;
using Clash.Maths;

namespace Clash.Input.Tests {
  // -- tests --
  public class StreamTests {
    [Test]
    public void ItFiltersTheAnalogStickDeadZone() {
      var source = Substitute.For<ISource>();
      var stream = new Stream(source);

      source.GetAxis("MoveX").Returns(0.15f);
      source.GetAxis("MoveY").Returns(0.10f);

      stream.OnUpdate(0.0f);
      var move = stream.GetCurrent().Move;
      Assert.That(move.Position, Is.EqualTo(Vec.Zero));
      Assert.That(move.RawPosition, Is.EqualTo(new Vec(0.15f, 0.10f)));
    }
  }
}
