using System;

namespace Clash.Player.Controls {
  using Ext;
  using K = Config;

  // -- types --
  public enum StateW {
    Possible,
    Pending,
    Satisfied,
    Failed
  }

  public enum ActionW {
    None,
    Wavedash,
    JumpA,
    JumpB
  }

  // -- impls --
  public sealed class WaveDash {
    // -- props --
    public ActionW Action { get; private set; }
    public StateW State { get; private set; }

    // -- props/intermdiate-state
    private Buttons buttons;
    private int failOnFrame;

    // -- commands --
    public void Reset() {
      Action = ActionW.None;
      State = StateW.Possible;
      buttons = Buttons.None;
      failOnFrame = -1;
    }

    // -- events --
    public void OnUpdate(Input.IStream inputs, int frame) {
      // if we recognized an action, stop
      if (Action != ActionW.None) {
        return;
      }

      // A: update flags
      var input = inputs.GetCurrent();
      if (input.JumpA.IsDown()) {
        buttons |= Buttons.JumpA;
      }

      if (input.JumpB.IsDown()) {
        buttons |= Buttons.JumpB;
      }

      if (input.ShieldL.IsDown()) {
        buttons |= Buttons.ShieldL;
      }

      if (input.ShieldR.IsDown()) {
        buttons |= Buttons.ShieldR;
      }

      // B: update wavedash state
      if (State == StateW.Possible || State == StateW.Pending) {
        // determine next state
        var nextS = StateW.Possible;

        // fire a wavedash on any two-button chord involving a jump button
        var isChordRecognized =
          (buttons.Has(Buttons.JumpA) && buttons.Any(~Buttons.JumpA)) ||
          (buttons.Has(Buttons.JumpB) && buttons.Any(~Buttons.JumpB));

        if (isChordRecognized) {
          nextS = StateW.Satisfied;
        } else if (State == StateW.Pending && frame >= failOnFrame) {
          nextS = StateW.Failed;
        } else if (buttons != Buttons.None) {
          nextS = StateW.Pending;
        }

        // set frame window on transition to pending
        if (nextS == StateW.Pending && State != nextS) {
          failOnFrame = frame + K.WaveDashFrameWindow;
        }

        // and set the new state
        State = nextS;
      }

      // C: update action
      var nextA = Action;

      if (State == StateW.Satisfied) {
        nextA = ActionW.Wavedash;
      } else if (State == StateW.Failed) {
        if (buttons.Has(Buttons.JumpA)) {
          nextA = ActionW.JumpA;
        } else if (buttons.Has(Buttons.JumpB)) {
          nextA = ActionW.JumpB;
        }
      }

      Action = nextA;
    }

    // -- types (internal) --
    [Flags]
    private enum Buttons {
      None = 0,
      JumpA = 1 << 0,
      JumpB = 1 << 2,
      ShieldL = 1 << 3,
      ShieldR = 1 << 4
    }
  }
}
