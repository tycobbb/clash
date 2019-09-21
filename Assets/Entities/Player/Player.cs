using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using U = UnityEngine;
using Input.Ext;

namespace Player {
  public sealed class Player {
    // -- core --
    public U.Vector2 mVelocity = U.Vector2.zero;
    public U.Vector2 mForce = U.Vector2.zero;

    // -- state-machine --
    private State mState;

    // -- events --
    public void OnStart(bool isAirborne) {
      if (isAirborne) {
        mState = new Airborne(isFalling: true);
      } else {
        mState = new Idle();
      }
    }

    public void OnPreUpdate(U.Vector2 v) {
      mState.AdvanceFrame();

      // sync body data
      mForce = U.Vector2.zero;
      mVelocity = new U.Vector2(v.x, v.y);
    }

    public void OnUpdate(Input.IStream inputs) {
      var input = inputs.GetCurrent();

      // check for physics-based state changes
      if (mState is Airborne && mVelocity.y <= 0.0f) {
        Fall();
      }

      // handle button inputs
      if (input.mJumpA.IsDown()) {
        OnFullJumpDown();
      } else if (input.mJumpB.IsDown()) {
        OnShortJumpDown();
      }

      // tick through any state changes
      if (mState is JumpWait) {
        OnJumpWait(inputs);
      } else if (mState is Dash) {
        OnDashWait(inputs);
      }

      // handle analog stick movement
      OnMoveX(inputs);
      OnMoveY(inputs);
    }

    public void OnPostSimulation(U.Vector2 v) {
      mVelocity = v;

      if (mState is Airborne) {
        LimitAirSpeed();
      }
    }

    // -- events/jump
    void OnFullJumpDown() {
      if (!(mState is JumpWait)) {
        StartFullJump();
      }
    }

    void OnShortJumpDown() {
      if (!(mState is JumpWait)) {
        StartShortJump();
      }
    }

    void OnJumpWait(Input.IStream inputs) {
      var jump = mState as JumpWait;
      var input = inputs.GetCurrent();

      // switch to short jump if button is released within the frame window
      if (!jump.IsShort && !input.mJumpA.IsActive()) {
        jump.IsShort = true;
      }

      // jump when frame window elapses
      if (jump.Frame >= K.JumpWaitFrames) {
        Jump();
      }
    }

    // -- events/move
    void OnMoveX(Input.IStream inputs) {
      var stick = inputs.GetCurrent().mMove;

      // capture move strength
      var mag = stick.mPosition.x;

      // fire airborne commands
      if (mState is Airborne) {
        Drift(mag);
      }
      // fire dash commands
      else if (mState is Dash) {
        if (IsHardSwitch(stick, Input.Direction.Horizontal)) {
          Dash(stick.mDirection);
        }
      }
      // fire run commands
      else if (mState is Run) {
        Run(stick.mDirection);
      }
      // fire move commands
      else if (!(mState is JumpWait)) {
        if (IsHardSwitch(stick, Input.Direction.Horizontal)) {
          Dash(stick.mDirection);
        } else {
          Walk(mag);
        }
      }
    }

    void OnMoveY(Input.IStream inputs) {
      var stick = inputs.GetCurrent().mMove;

      var mag = stick.mPosition.y;
      if (mag == 0.0f) {
        return;
      }

      var airborne = mState as Airborne;
      if (airborne?.IsFalling == true && IsHardSwitch(stick, Input.Direction.Down)) {
        FastFall();
      }
    }

    // -- events/run
    void OnDashWait(Input.IStream inputs) {
      var dash = mState as Dash;
      if (dash.Frame < K.DashFrames) {
        return;
      }

      // enter run if dash and current direction are the same
      var stick = inputs.GetCurrent().mMove;
      if (dash.Direction == stick.mDirection) {
        Run(dash.Direction);
      } else {
        // TODO: enter a skid/stop jump
        SwitchState(new Idle());
      }
    }

    // -- events/helpers
    private bool IsHardSwitch(Input.Analog stick, Input.Direction direction) {
      if (!stick.mDirection.Intersects(direction)) {
        return false;
      }

      var pos = stick.mPosition;
      var mag = U.Mathf.Abs(direction.IsHorizontal() ? pos.x : pos.y);

      return mag >= 0.8f;
    }

    // -- commands --
    // -- commands/run
    void Dash(Input.Direction direction) {
      var jump = mState as Dash;

      // ignore repeat dashes, but allow dash back
      if (jump?.Direction == direction) {
        return;
      }

      SwitchState(new Dash(direction));
      mVelocity = new U.Vector2(direction.IsLeft() ? -K.Dash : K.Dash, 0.0f);
    }

    void Run(Input.Direction direction) {
      var run = mState as Run;
      if (run == null) {
        run = new Run(direction);
        SwitchState(run);
      }

      // stay in run if it matches the current state
      if (run.Direction == direction) {
        mVelocity = new U.Vector2(direction.IsLeft() ? -K.Run : K.Run, 0.0f);
      } else {
        // TODO: switch to "Stopping" state, Idling when v.x = 0
        SwitchState(new Idle());
      }
    }

    void Walk(float xMove) {
      if (xMove == 0.0f) {
        if (!(mState is Idle)) {
          SwitchState(new Idle());
        }

        return;
      }

      // TODO: should SwitchState ignore duplicate states, and also, what is a duplicate?
      if (!(mState is Walk)) {
        SwitchState(new Walk());
      }

      mVelocity = new U.Vector2(xMove * K.Walk, 0.0f);
    }

    // -- commands/jump
    void StartFullJump() {
      SwitchState(new JumpWait(isShort: false));
    }

    void StartShortJump() {
      SwitchState(new JumpWait(isShort: true));
    }

    void Jump() {
      var jump = mState as JumpWait;
      SwitchState(new Airborne(isFalling: false));
      mVelocity = mVelocity.WithY(jump.IsShort ? K.JumpShort : K.Jump);
    }

    void Drift(float xMov) {
      mForce.x += xMov * K.Drift;
    }

    void Fall() {
      var airborne = mState as Airborne;
      airborne.IsFalling = true;
    }

    void FastFall() {
      mVelocity = mVelocity.WithY(-K.FastFall);
    }

    public void Land() {
      var airborne = mState as Airborne;
      if (airborne?.IsFalling != true) {
        return;
      }

      SwitchState(new Idle());
    }

    void LimitAirSpeed() {
      var v = mVelocity;
      mVelocity = new U.Vector2(U.Mathf.Clamp(v.x, -K.MaxAirSpeedX, K.MaxAirSpeedX), v.y);
    }

    private void SwitchState(State state) {
      Log.Debug("[Player] State Switch: " + state);
      mState = state;
    }
  }
}
