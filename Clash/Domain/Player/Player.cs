using Clash.Maths;
using Clash.Input.Ext;

namespace Clash.Player {
  using K = Config;

  public sealed class Player {
    // -- core --
    public Vec Velocity = Vec.Zero;
    public Vec Force = Vec.Zero;

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

    public void OnPreUpdate(Vec v) {
      state.AdvanceFrame();

      // sync body data
      Force = Vec.Zero;
      Velocity = new Vec(v.X, v.Y);
    }

    public void OnUpdate(Input.IStream inputs) {
      var input = inputs.GetCurrent();

      // check for physics-based state changes
      if (state is Airborne && Velocity.Y <= 0.0f) {
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
        case Pivot p:
          OnPivot(p, inputs); break;
        case Skid _:
          OnSkid(inputs); break;
        case JumpWait j:
          OnJumpWait(j, inputs); break;
        case Airborne a:
          OnAirborne(a, inputs); break;
      }
    }

    public void OnPostSimulation(Vec v) {
      // sync body data
      Force = Vec.Zero;
      Velocity = new Vec(v.X, v.Y);

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

      if (DidTap(stick, Input.Direction.Horizontal)) {
        Dash(stick.Direction, stick.Position.X);
      } else {
        Walk(stick.Position.X);
      }
    }

    // -- events/move
    void OnWalk(Input.IStream inputs) {
      var stick = inputs.GetCurrent().Move;

      if (DidTap(stick, Input.Direction.Horizontal)) {
        Dash(stick.Direction, stick.Position.X);
      } else {
        Walk(stick.Position.X);
      }
    }

    void OnDash(Dash dash, Input.IStream inputs) {
      var stick = inputs.GetCurrent().Move;

      // check for a dash back
      if (DidTap(stick, dash.Direction.Invert())) {
        Dash(stick.Direction, stick.Position.X);
      }
      // this dash is incomplete, so keep dashing
      else if (dash.Frame < K.DashFrames) {
        Dash(dash.Direction, stick.Position.X);
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

    void OnPivot(Pivot pivot, Input.IStream _) {
      if (pivot.Frame >= K.RunPivotFrames) {
        Idle();
      }
    }

    void OnSkid(Input.IStream inputs) {
      var stick = inputs.GetCurrent().Move;

      if (DidTap(stick, Input.Direction.Horizontal)) {
        Dash(stick.Direction, stick.Position.X);
      } else if (stick.Position.X != 0.0f) {
        Walk(stick.Position.X);
      } else if (Velocity.X == 0.0f) {
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
      Drift(stick.Position.X);

      // fastfall on a fresh down input
      if (airborne?.IsFalling == true && DidTap(stick, Input.Direction.Down)) {
        FastFall();
      }
    }

    void OnAirborneLate() {
      LimitAirSpeed();
    }

    // -- events/helpers
    private bool DidTap(Input.Analog stick, Input.Direction direction) {
      return stick.DidTap() && stick.Direction.Intersects(direction);
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
        Velocity = new Vec(dir.IsLeft() ? -K.DashInitial : K.DashInitial, 0.0f);
      }

      // apply dash force based on xAxis
      var isAxisAligned =
        xAxis < 0 && dir.IsLeft() ||
        xAxis > 0 && dir.IsRight();

      var scale = isAxisAligned ? Mathf.Abs(xAxis) : 0.0f;
      var force = K.DashBase + K.DashScale * scale;

      Force.X += dir.IsLeft() ? -force : force;
    }

    void LimitGroundSpeed() {
      var v = Velocity;
      Velocity = new Vec(Mathf.Clamp(v.X, -K.Run, K.Run), v.Y);
    }

    void Run(Input.Direction direction) {
      var run = state as Run;
      if (run == null) {
        run = new Run(direction);
        SwitchState(run);
      }

      // stay in run if it matches the current state
      if (run.Direction == direction) {
        Velocity = new Vec(direction.IsLeft() ? -K.Run : K.Run, 0.0f);
      } else {
        Pivot();
      }
    }

    void Pivot() {
      SwitchState(new Pivot());
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

      Velocity = new Vec(xMove * K.Walk, 0.0f);
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
      Velocity = new Vec(Velocity.X, jump.IsShort ? K.JumpShort : K.Jump);
    }

    void Drift(float xMov) {
      Force.X += xMov * K.Drift;
    }

    void Fall() {
      var airborne = state as Airborne;
      airborne.IsFalling = true;
    }

    void FastFall() {
      Velocity = new Vec(Velocity.X, -K.FastFall);
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
      Velocity = new Vec(Mathf.Clamp(v.X, -K.MaxAirSpeedX, K.MaxAirSpeedX), v.Y);
    }

    private void SwitchState(State state) {
      Log.Debug($"[Player] SwitchState({state})");
      this.state = state;
    }
  }
}
