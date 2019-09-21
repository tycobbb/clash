using U = UnityEngine;
using Input.Ext;

namespace Player {
  using K = Config;

  public sealed class Player {
    // -- core --
    public U.Vector2 Velocity = U.Vector2.zero;
    public U.Vector2 Force = U.Vector2.zero;

    // -- state-machine --
    private State state;

    // -- events --
    public void OnStart(bool isAirborne) {
      if (isAirborne) {
        state = new Airborne(isFalling: true);
      } else {
        state = new Idle();
      }
    }

    public void OnPreUpdate(U.Vector2 v) {
      state.AdvanceFrame();

      // sync body data
      Force = U.Vector2.zero;
      Velocity = new U.Vector2(v.x, v.y);
    }

    public void OnUpdate(Input.IStream inputs) {
      var input = inputs.GetCurrent();

      // check for physics-based state changes
      if (state is Airborne && Velocity.y <= 0.0f) {
        Fall();
      }

      // handle button inputs
      if (input.JumpA.IsDown()) {
        OnFullJumpDown();
      } else if (input.JumpB.IsDown()) {
        OnShortJumpDown();
      }

      // tick through any state changes
      if (state is JumpWait) {
        OnJumpWait(inputs);
      } else if (state is Dash) {
        OnDashWait(inputs);
      }

      // handle analog stick movement
      OnMoveX(inputs);
      OnMoveY(inputs);
    }

    public void OnPostSimulation(U.Vector2 v) {
      Velocity = v;

      if (state is Airborne) {
        LimitAirSpeed();
      }
    }

    // -- events/jump
    void OnFullJumpDown() {
      if (state is JumpWait || state is Airborne) {
        return;
      }

      StartFullJump();
    }

    void OnShortJumpDown() {
      if (state is JumpWait || state is Airborne) {
        return;
      }

      StartShortJump();
    }

    void OnJumpWait(Input.IStream inputs) {
      var jump = state as JumpWait;
      var input = inputs.GetCurrent();

      // switch to short jump if button is released within the frame window
      if (!jump.IsShort && !input.JumpA.IsActive()) {
        jump.IsShort = true;
      }

      // jump when frame window elapses
      if (jump.Frame >= K.JumpWaitFrames) {
        Jump();
      }
    }

    // -- events/move
    void OnMoveX(Input.IStream inputs) {
      var stick = inputs.GetCurrent().Move;

      // capture move strength
      var mag = stick.Position.x;

      // fire airborne commands
      if (state is Airborne) {
        Drift(mag);
      }
      // fire dash commands
      else if (state is Dash) {
        if (IsHardSwitch(stick, Input.Direction.Horizontal)) {
          Dash(stick.Direction);
        }
      }
      // fire run commands
      else if (state is Run) {
        Run(stick.Direction);
      }
      // fire move commands
      else if (!(state is JumpWait)) {
        if (IsHardSwitch(stick, Input.Direction.Horizontal)) {
          Dash(stick.Direction);
        } else {
          Walk(mag);
        }
      }
    }

    void OnMoveY(Input.IStream inputs) {
      var stick = inputs.GetCurrent().Move;

      var mag = stick.Position.y;
      if (mag == 0.0f) {
        return;
      }

      var airborne = state as Airborne;
      if (airborne?.IsFalling == true && IsHardSwitch(stick, Input.Direction.Down)) {
        FastFall();
      }
    }

    // -- events/run
    void OnDashWait(Input.IStream inputs) {
      var dash = state as Dash;
      if (dash.Frame < K.DashFrames) {
        return;
      }

      // enter run if dash and current direction are the same
      var stick = inputs.GetCurrent().Move;
      if (dash.Direction == stick.Direction) {
        Run(dash.Direction);
      } else {
        // TODO: enter a skid/stop jump
        SwitchState(new Idle());
      }
    }

    // -- events/helpers
    private bool IsHardSwitch(Input.Analog stick, Input.Direction direction) {
      if (!stick.Direction.Intersects(direction)) {
        return false;
      }

      var pos = stick.Position;
      var mag = U.Mathf.Abs(direction.IsHorizontal() ? pos.x : pos.y);

      return mag >= 0.8f;
    }

    // -- commands --
    // -- commands/run
    void Dash(Input.Direction direction) {
      var jump = state as Dash;

      // ignore repeat dashes, but allow dash back
      if (jump?.Direction == direction) {
        return;
      }

      SwitchState(new Dash(direction));
      Velocity = new U.Vector2(direction.IsLeft() ? -K.Dash : K.Dash, 0.0f);
    }

    void Run(Input.Direction direction) {
      var run = state as Run;
      if (run == null) {
        run = new Run(direction);
        SwitchState(run);
      }

      // stay in run if it matches the current state
      if (run.Direction == direction) {
        Velocity = new U.Vector2(direction.IsLeft() ? -K.Run : K.Run, 0.0f);
      } else {
        // TODO: switch to "Stopping" state, Idling when v.x = 0
        SwitchState(new Idle());
      }
    }

    void Walk(float xMove) {
      if (xMove == 0.0f) {
        if (!(state is Idle)) {
          SwitchState(new Idle());
        }

        return;
      }

      // TODO: should SwitchState ignore duplicate states, and also, what is a duplicate?
      if (!(state is Walk)) {
        SwitchState(new Walk());
      }

      Velocity = new U.Vector2(xMove * K.Walk, 0.0f);
    }

    // -- commands/jump
    void StartFullJump() {
      SwitchState(new JumpWait(isShort: false));
    }

    void StartShortJump() {
      SwitchState(new JumpWait(isShort: true));
    }

    void Jump() {
      var jump = state as JumpWait;
      SwitchState(new Airborne(isFalling: false));
      Velocity = Velocity.WithY(jump.IsShort ? K.JumpShort : K.Jump);
    }

    void Drift(float xMov) {
      Force.x += xMov * K.Drift;
    }

    void Fall() {
      var airborne = state as Airborne;
      airborne.IsFalling = true;
    }

    void FastFall() {
      Velocity = Velocity.WithY(-K.FastFall);
    }

    public void Land() {
      var airborne = state as Airborne;
      if (airborne?.IsFalling != true) {
        return;
      }

      SwitchState(new Idle());
    }

    void LimitAirSpeed() {
      var v = Velocity;
      Velocity = new U.Vector2(U.Mathf.Clamp(v.x, -K.MaxAirSpeedX, K.MaxAirSpeedX), v.y);
    }

    private void SwitchState(State state) {
      Log.Debug("[Player] State Switch: " + state);
      this.state = state;
    }
  }
}
