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

      // handle per-state updates
      switch (state) {
        case Idle _:
          OnIdle(inputs); break;
        case Walk _:
          OnWalk(inputs); break;
        case Dash d:
          OnDash(d, inputs); break;
        case Run _:
          OnRun(inputs); break;
        case Skid _:
          OnSkid(inputs); break;
        case JumpWait j:
          OnJumpWait(j, inputs); break;
        case Airborne a:
          OnAirborne(a, inputs); break;
      }
    }

    public void OnPostSimulation(U.Vector2 v) {
      // sync body data
      Force = U.Vector2.zero;
      Velocity = new U.Vector2(v.x, v.y);

      // handle per-state updates
      switch (state) {
        case Dash _:
          OnDashLate(); break;
        case Airborne _:
          OnAirborneLate(); break;
      }
    }

    // -- events/neutral
    void OnIdle(Input.IStream inputs) {
      var stick = inputs.GetCurrent().Move;

      if (IsHardSwitch(stick, Input.Direction.Horizontal)) {
        Dash(stick.Direction, stick.Position.x);
      } else {
        Walk(stick.Position.x);
      }
    }

    // -- events/move
    void OnWalk(Input.IStream inputs) {
      var stick = inputs.GetCurrent().Move;

      if (IsHardSwitch(stick, Input.Direction.Horizontal)) {
        Dash(stick.Direction, stick.Position.x);
      } else {
        Walk(stick.Position.x);
      }
    }

    void OnDash(Dash dash, Input.IStream inputs) {
      var stick = inputs.GetCurrent().Move;

      // check for a dash back
      if (IsHardSwitch(stick, dash.Direction.Invert())) {
        Dash(stick.Direction, stick.Position.x);
      }
      // this dash is incomplete, so keep dashing
      else if (dash.Frame < K.DashFrames) {
        Dash(dash.Direction, stick.Position.x);
      }
      // otherwise, transition out of dash
      else if (stick.Direction == dash.Direction) {
        Run(dash.Direction);
      } else {
        Skid();
      }
    }

    void OnDashLate() {
      LimitGroundSpeed();
    }

    void OnRun(Input.IStream inputs) {
      var stick = inputs.GetCurrent().Move;
      Run(stick.Direction);
    }

    void OnSkid(Input.IStream inputs) {
      var stick = inputs.GetCurrent().Move;

      if (IsHardSwitch(stick, Input.Direction.Horizontal)) {
        Dash(stick.Direction, stick.Position.x);
      } else if (stick.Position.x != 0.0f) {
        Walk(stick.Position.x);
      } else if (Velocity.x == 0.0f) {
        Idle();
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

    void OnJumpWait(JumpWait jump, Input.IStream inputs) {
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

    void OnAirborne(Airborne airborne, Input.IStream inputs) {
      // add air control
      var stick = inputs.GetCurrent().Move;
      Drift(stick.Position.x);

      // fastfall on a fresh down input
      if (airborne?.IsFalling == true && IsHardSwitch(stick, Input.Direction.Down)) {
        FastFall();
      }
    }

    void OnAirborneLate() {
      LimitAirSpeed();
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
    void Idle() {
      SwitchState(new Idle());
    }

    // -- commands/walk&run
    // See: https://www.ssbwiki.com/Dash
    void Dash(Input.Direction dir, float xAxis) {
      var dash = state as Dash;

      // if a new dash or direction change, set state and initial velocity
      if (dash == null || dash.Direction != dir) {
        dash = new Dash(dir);
        SwitchState(dash);
        Velocity = new U.Vector2(dir.IsLeft() ? -K.DashInitial : K.DashInitial, 0.0f);
      }

      // apply dash force based on xAxis
      var isAxisAligned =
        xAxis < 0 && dir.IsLeft() ||
        xAxis > 0 && dir.IsRight();

      var scale = isAxisAligned ? U.Mathf.Abs(xAxis) : 0.0f;
      var force = K.DashBase + K.DashScale * scale;

      Force.x += dir.IsLeft() ? -force : force;
    }

    void LimitGroundSpeed() {
      var v = Velocity;
      Velocity = new U.Vector2(U.Mathf.Clamp(v.x, -K.Run, K.Run), v.y);
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
        // TODO: enter turnaround
        Skid();
      }
    }

    void Skid() {
      SwitchState(new Skid());
    }

    void Walk(float xMove) {
      if (xMove == 0.0f) {
        if (!(state is Idle)) {
          Idle();
        }

        return;
      }

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
