using Clash.Maths;
using Clash.Input.Ext;

namespace Clash.Player {
  using K = Config;

  public sealed class Player {
    // -- physics --
    public Vec Velocity;
    public Vec Force;
    public float Gravity = K.GravityOn;

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
      // handle per-state events
      switch (State) {
        case Idle _:
          OnIdle(inputs); break;
        case Walk _:
          OnWalk(inputs); break;
        case Dash d:
          OnDash(d, inputs); break;
        case Run r:
          OnRun(r, inputs); break;
        case Pivot p:
          OnPivot(p, inputs); break;
        case Skid _:
          OnSkid(inputs); break;
        case JumpWait j:
          OnJumpWait(j, inputs); break;
        case Airborne a:
          OnAirborne(a, inputs); break;
        case AirDodge _:
          OnAirDodge(); break;
        case WaveLand _:
          OnWaveLand(); break;
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
        case AirDodge a:
          OnAirDodgeLate(a); break;
      }
    }

    public void OnCollide() {
      switch (State) {
        case Airborne a:
          OnAirborneCollide(a); break;
        case AirDodge _:
          OnAirDodgeCollide(); break;
      }
    }

    // -- events/neutral
    void OnIdle(Input.IStream inputs) {
      var input = inputs.GetCurrent();
      var stick = input.Move;

      if (input.JumpA.IsDown()) {
        FullJump();
      } else if (input.JumpB.IsDown()) {
        ShortJump();
      } else if (DidTap(stick, Input.Direction.Horizontal)) {
        Dash(stick.Direction, stick.Position.X);
      } else if (stick.Position.X != 0.0f) {
        Walk(stick.Direction, stick.Position.X);
      }
    }

    // -- events/move
    void OnWalk(Input.IStream inputs) {
      var input = inputs.GetCurrent();
      var stick = input.Move;

      // check for jumps
      if (input.JumpA.IsDown()) {
        FullJump();
      } else if (input.JumpB.IsDown()) {
        ShortJump();
      }
      // dash on tap
      else if (DidTap(stick, Input.Direction.Horizontal)) {
        Dash(stick.Direction, stick.Position.X);
      }
      // walk/idle on any other stick input
      else if (stick.Position.X != 0.0f) {
        Walk(stick.Direction, stick.Position.X);
      } else {
        Idle();
      }
    }

    void OnDash(Dash dash, Input.IStream inputs) {
      var input = inputs.GetCurrent();
      var stick = input.Move;

      // check for jumps
      if (input.JumpA.IsDown()) {
        FullJump();
      } else if (input.JumpB.IsDown()) {
        ShortJump();
      }
      // dash back if tapping the opposite direction
      else if (DidTap(stick, dash.Direction.Reversed())) {
        Dash(stick.Direction, stick.Position.X);
      }
      // if this dash is incomplete, keep dashing
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

    void OnRun(Run run, Input.IStream inputs) {
      var input = inputs.GetCurrent();
      var stick = input.Move;

      // check for jumps
      if (input.JumpA.IsDown()) {
        FullJump();
      } else if (input.JumpB.IsDown()) {
        ShortJump();
      }
      // stay in run if stick direction is the same
      else if (stick.Direction == run.Direction) {
        Run(run.Direction);
      }
      // pivot if the stick direction is opposite the run direction
      else if (stick.Direction == run.Direction.Reversed()) {
        Pivot(stick.Direction);
      }
      // otherwise stop running
      else {
        Skid();
      }
    }

    void OnPivot(Pivot pivot, Input.IStream inputs) {
      var input = inputs.GetCurrent();
      var stick = input.Move;

      // check for jumps
      if (input.JumpA.IsDown()) {
        FullJump();
      } else if (input.JumpB.IsDown()) {
        ShortJump();
      }
      // don't do anything else until pivot finishes
      else if (pivot.Frame < K.RunPivotFrames) {
        return;
      }
      // start running if holding pivot direction
      else if (stick.Direction == pivot.Direction) {
        Run(pivot.Direction);
      }
      // otherwise just stop
      else {
        Idle();
      }
    }

    void OnSkid(Input.IStream inputs) {
      var input = inputs.GetCurrent();
      var stick = input.Move;

      // check for jumps
      if (input.JumpA.IsDown()) {
        FullJump();
      } else if (input.JumpB.IsDown()) {
        ShortJump();
      }
      // dash on tap
      else if (DidTap(stick, Input.Direction.Horizontal)) {
        Dash(stick.Direction, stick.Position.X);
      }
      // walk/idle on any other stick input
      else if (stick.Position.X != 0.0f) {
        Walk(stick.Direction, stick.Position.X);
      } else {
        Idle();
      }
    }

    // -- events/jump
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
      // otherwise wavedash if shield is pressed
      else if (input.ShieldL.IsDown() || input.ShieldR.IsDown()) {
        var stick = input.Move;
        WaveDash(stick.Position.Normalize());
      }
    }

    void OnAirborne(Airborne airborne, Input.IStream inputs) {
      var input = inputs.GetCurrent();
      var stick = input.Move;

      // add air control
      Drift(stick.Position.X);

      // fall if moving downwards
      if (Velocity.Y <= 0.0f) {
        Fall();
      }

      // fastfall on a downwards tap
      if (airborne.IsFalling && DidTap(stick, Input.Direction.Down)) {
        FastFall();
      }

      // airdodge if shield is pressed
      if (input.ShieldL.IsDown() || input.ShieldR.IsDown()) {
        StartAirDodge(stick.Position.Normalize());
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

    void OnAirDodge() {
      AirDodge();
    }

    void OnAirDodgeLate(AirDodge airDodge) {
      var minFrames = (int)Mathf.Ceil(K.AirDodge / K.AirDodgeDecay);
      if (airDodge.Frame < minFrames) {
        return;
      }

      // do a coarse check to see if the player and airdodge directions
      // oppose each other
      var direction = airDodge.Direction;
      var isDirectionOpposed = (
        Mathf.Sign(Velocity.X) == -Mathf.Sign(direction.X) &&
        Mathf.Sign(Velocity.Y) == -Mathf.Sign(direction.Y)
      );

      // if so, enter fall state
      if (isDirectionOpposed) {
        EndAirDodge();
        Velocity = Vec.Zero;
        Fall();
      }
    }

    void OnAirDodgeCollide() {
      EndAirDodge();
      WaveLand();
    }

    void OnWaveLand() {
      if (Velocity.X == 0.0f) {
        Idle();
      }
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
      var isAxisAligned = (
        xAxis < 0 && dir.IsLeft() ||
        xAxis > 0 && dir.IsRight()
      );

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

      Velocity = new Vec(direction.IsLeft() ? -K.Run : K.Run, 0.0f);
    }

    void Pivot(Input.Direction direction) {
      SwitchState(new Pivot(direction));
      IsFacingLeft = direction.IsLeft();
    }

    void Skid() {
      SwitchState(new Skid());
    }

    // -- commands/jump
    void FullJump() {
      SwitchState(new JumpWait(isShort: false));
    }

    void ShortJump() {
      SwitchState(new JumpWait(isShort: true));
    }

    void StartAirDodge(Vec direction) {
      // TODO: use a larger "dead zone" on air dodge so inputs that aren't at the
      // edges of the stickbox register as a neutral air dodge
      var airDodge = new AirDodge(direction);
      SwitchState(airDodge);

      // disable gravity while air dodging
      Gravity = K.GravityOff;

      // cancel momentum and apply the initial force
      Velocity = Vec.Zero;
      Force += airDodge.Direction * K.AirDodge;
    }

    void AirDodge() {
      var airDodge = State as AirDodge;
      // decay air dodge each frame
      Force += airDodge.Direction.Reverse() * K.AirDodgeDecay;
    }

    void EndAirDodge() {
      // re-enable gravity
      Gravity = K.GravityOn;
    }

    void WaveDash(Vec direction) {
      // cancel momentum and apply the initial force
      Velocity = Vec.Zero;
      Force += direction * K.AirDodge;

      // transition straight to wave land
      WaveLand();
    }

    void WaveLand() {
      var state = new WaveLand();
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
      Log.Debug($"[Player] SwitchState({State} => {state})");
      State = state;
    }
  }
}
