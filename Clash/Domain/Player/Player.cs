using Clash.Maths;
using Clash.Input.Ext;

namespace Clash.Player {
  using K = Config;

  public sealed class Player {
    // -- physics --
    public Vec Velocity;
    public Vec Force;

    // -- state machine --
    public State State { get; private set; }

    // -- properties --
    public bool IsFacingLeft { get; private set; }

    // -- lifetime --
    public Player(bool isOnGround) {
      if (isOnGround) {
        State = new Idle();
      } else {
        State = new Airborne(isFalling: true);
      }
    }

    // -- events --
    public void OnPreUpdate(Vec v) {
      State.AdvanceFrame();

      // sync body data
      Force = Vec.Zero;
      Velocity = new Vec(v.X, v.Y);
    }

    public void OnUpdate(Input.IStream inputs) {
      var input = inputs.GetCurrent();

      // check for physics-based state changes
      if (State is Airborne && Velocity.Y <= 0.0f) {
        Fall();
      }

      // handle button inputs
      if (input.JumpA.IsDown()) {
        OnFullJumpDown();
      } else if (input.JumpB.IsDown()) {
        OnShortJumpDown();
      } else if (input.ShieldL.IsDown() || input.ShieldR.IsDown()) {
        OnShieldDown(inputs);
      }

      // handle per-state updates
      switch (State) {
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
        case AirDodge a:
          OnAirDodge(a); break;
      }
    }

    public void OnPostSimulation(Vec v) {
      // sync body data
      Force = Vec.Zero;
      Velocity = new Vec(v.X, v.Y);

      // handle per-state updates
      switch (State) {
        case Dash _:
          OnDashLate(); break;
        case Airborne _:
          OnAirborneLate(); break;
      }
    }

    public void OnCollide() {
      switch (State) {
        case Airborne a:
          OnAirborneCollide(a); break;
        case AirDodge a:
          OnAirDodgeCollide(a); break;
      }
    }

    // -- events/neutral
    void OnIdle(Input.IStream inputs) {
      var stick = inputs.GetCurrent().Move;

      if (DidTap(stick, Input.Direction.Horizontal)) {
        Dash(stick.Direction, stick.Position.X);
      } else if (stick.Position.X != 0.0f) {
        Walk(stick.Direction, stick.Position.X);
      }
    }

    // -- events/move
    void OnWalk(Input.IStream inputs) {
      var stick = inputs.GetCurrent().Move;

      if (DidTap(stick, Input.Direction.Horizontal)) {
        Dash(stick.Direction, stick.Position.X);
      } else if (stick.Position.X != 0.0f) {
        Walk(stick.Direction, stick.Position.X);
      } else {
        Idle();
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
        Walk(stick.Direction, stick.Position.X);
      } else if (Velocity.X == 0.0f) {
        Idle();
      }
    }

    // -- events/jump
    void OnFullJumpDown() {
      if (State is JumpWait || State is Airborne) {
        return;
      }

      StartFullJump();
    }

    void OnShortJumpDown() {
      if (State is JumpWait || State is Airborne) {
        return;
      }

      StartShortJump();
    }

    void OnShieldDown(Input.IStream inputs) {
      var stick = inputs.GetCurrent().Move;
      var direction = stick.Position.Normalize();

      switch (State) {
        case JumpWait _:
          StartWavedash(direction); break;
        case Airborne _:
          StartAirDodge(direction); break;
      }
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

    void OnAirborneCollide(Airborne airborne) {
      if (airborne.IsFalling) {
        Land();
      }
    }

    void OnAirborneLate() {
      LimitAirSpeed();
    }

    void OnAirDodge(AirDodge airDodge) {
      var frame = airDodge.Frame;

      // stop momentum and fire dodge on frame 0
      if (frame == 0) {
        Velocity = Vec.Zero;
        Force += airDodge.Direction * K.AirDodge;
      }
      // TODO: enter helpless when finished
      else if (frame >= K.AirDodgeFrames) {
        if (airDodge.IsOnGround) {
          Idle();
        } else {
          Fall();
        }
      }
    }

    void OnAirDodgeCollide(AirDodge airDodge) {
      airDodge.IsOnGround = true;
    }

    // -- events/helpers
    private bool DidTap(Input.Analog stick, Input.Direction direction) {
      return stick.DidTap() && stick.Direction.Intersects(direction);
    }

    // -- commands --
    void Idle() {
      SwitchState(new Idle());
    }

    // -- commands/move
    void Walk(Input.Direction direction, float xMove) {
      if (!(State is Walk)) {
        SwitchState(new Walk());
      }

      Velocity = new Vec(xMove * K.Walk, 0.0f);
      IsFacingLeft = direction.IsLeft();
    }

    // See: https://www.ssbwiki.com/Dash
    void Dash(Input.Direction dir, float xAxis) {
      var dash = State as Dash;

      // if new dash or direction change, set state
      if (dash == null || dash.Direction != dir) {
        dash = new Dash(dir);
        SwitchState(dash);
      }

      // if a new dash or direction change, set initial velocity
      if (dash.Frame == 0) {
        Velocity = new Vec(dir.IsLeft() ? -K.DashInitial : K.DashInitial, 0.0f);
        IsFacingLeft = dir.IsLeft();
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
      var run = State as Run;
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
      IsFacingLeft = !IsFacingLeft;
    }

    void Skid() {
      SwitchState(new Skid());
    }

    // -- commands/jump
    void StartFullJump() {
      SwitchState(new JumpWait(isShort: false));
    }

    void StartShortJump() {
      SwitchState(new JumpWait(isShort: true));
    }

    void StartAirDodge(Vec direction) {
      // TODO: use a larger "dead zone" on air dodge so inputs that aren't at the
      // edges of the stickbox register as a neutral air dodge
      var state = new AirDodge(direction);
      SwitchState(state);
    }

    void StartWavedash(Vec direction) {
      var state = new AirDodge(direction, isOnGround: true);
      SwitchState(state);
    }

    void Jump() {
      var jump = State as JumpWait;
      SwitchState(new Airborne(isFalling: false));
      Velocity = new Vec(Velocity.X, jump.IsShort ? K.JumpShort : K.Jump);
    }

    void Drift(float xMov) {
      Force.X += xMov * K.Drift;
    }

    void Fall() {
      var airborne = State as Airborne;
      if (airborne == null) {
        airborne = new Airborne(isFalling: true);
        SwitchState(airborne);
      }

      airborne.IsFalling = true;
    }

    void FastFall() {
      Velocity = new Vec(Velocity.X, -K.FastFall);
    }

    void Land() {
      // TODO: transition to landing state
      Idle();
    }

    void LimitAirSpeed() {
      var v = Velocity;
      Velocity = new Vec(Mathf.Clamp(v.X, -K.MaxAirSpeedX, K.MaxAirSpeedX), v.Y);
    }

    void SwitchState(State state) {
      Log.Debug($"[Player] SwitchState({state})");
      this.State = state;
    }
  }
}
