namespace Clash.Input {
  // -- types --
  public interface IGestureRecognizer {
    void Reset();
    StateG OnInput(Gesture gesture, IStream inputs);
  }

  public enum StateG {
    Possible,
    Pending,
    Satisfied,
    Failed
  }

  // -- impls --
  public sealed class Gesture {
    // -- dependencies --
    private readonly IGestureRecognizer recognizer;

    // -- properties --
    public StateG State { get; private set; }
    public bool IsRecognized { get; private set; }

    private Gesture prev;
    private Gesture next;

    // -- lifetime --
    public Gesture(IGestureRecognizer recognizer) {
      this.recognizer = recognizer;
    }

    // -- lifecycle --
    public void Reset() {
      State = StateG.Possible;

      // allow the recognizer to reset its state
      recognizer.Reset();

      // reset the next gesture
      if (next != null) {
        next.Reset();
      }
    }

    public void OnUpdate(IStream inputs) {
      // recognize if this gesture became unblocked
      InvalidateRecognition();

      // update the gesture if it hasn't completed
      if (State == StateG.Possible || State == StateG.Pending) {
        State = recognizer.OnInput(this, inputs);
        InvalidateRecognition();
      }

      // update the next gesture in the chain
      if (next != null) {
        next.OnUpdate(inputs);
      }
    }

    // -- commands --
    public void AddNext(Gesture next) {
      this.next = next;
      next.prev = this;
    }

    private void InvalidateRecognition() {
      IsRecognized = State == StateG.Satisfied && !IsBlocked();
    }

    // -- queries --
    private bool IsBlocked() {
      return (
        prev != null && (
          prev.State == StateG.Pending ||
          prev.State == StateG.Satisfied ||
          prev.IsBlocked()
        )
      );
    }
  }

  public abstract class GestureRecognizer: IGestureRecognizer {
    // -- IGestureRecognizer --
    public virtual void Reset() {
    }

    public abstract StateG OnInput(Gesture gesture, IStream inputs);
  }
}
