using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using U = UnityEngine;
using Input.Ext;

public sealed class Player {
  // -- core --
  public U.Vector2 mVelocity = U.Vector2.zero;
  public U.Vector2 mForce = U.Vector2.zero;

  // -- constants --
  public const float kGravity = 1.0f;
  private const int kJumpWaitFrames = 8;
  private const float kJump = 5.0f;
  private const float kJumpShort = 3.0f;
  private const float kFastFall = 6.0f;
  private const float kDash = 5.0f;
  private const float kDashFrames = 5;
  private const float kRun = 4.0f;
  private const float kWalk = 2.0f;
  private const float kDrift = 0.2f;
  private const float kMaxAirSpeedX = 3.0f;

  // -- state-machine --
  private State mState;

  // -- events --
  public void OnStart(bool isAirborne) {
    var state = isAirborne ? State.Type.Airborne : State.Type.Idling;
    mState = new State(state);
  }

  public void OnPreUpdate(U.Vector2 v) {
    mState.AdvanceFrame();

    // sync body data
    mForce = U.Vector2.zero;
    mVelocity = new U.Vector2(v.x, v.y);
  }

  public void OnUpdate(Input.IStream inputs) {
    var input = inputs.GetCurrent();

    // check for physiscs-based state changes
    if (mVelocity.y <= 0.0f && mState.Is(State.Type.Airborne)) {
      Fall();
    }

    // handle button inputs
    if (input.mJumpA.IsDown()) {
      OnFullJumpDown();
    } else if (input.mJumpB.IsDown()) {
      OnShortJumpDown();
    }

    // tick through any state changes
    if (mState.Any(State.kJumpStart)) {
      OnJumpWait(inputs);
    } else if (mState.Any(State.kDashing)) {
      OnDashWait(inputs);
    }

    // handle analog stick movement
    OnMoveX(inputs);
    OnMoveY(inputs);
  }

  public void OnPostSimulation(U.Vector2 v) {
    mVelocity = v;

    if (mState.Any(State.Type.Airborne)) {
      LimitAirSpeed();
    }
  }


  // -- events/jump
  void OnFullJumpDown() {
    if (!mState.Any(State.kProhibitsJump1)) {
      StartFullJump();
    }
  }

  void OnShortJumpDown() {
    if (!mState.Any(State.kProhibitsJump1)) {
      StartShortJump();
    }
  }

  void OnJumpWait(Input.IStream inputs) {
    var input = inputs.GetCurrent();

    // switch to short jump if jump is released within frame window
    if (mState.mType == State.Type.Jump1fWait && !input.mJumpA.IsActive()) {
      ReplaceState(State.Type.Jump1sWait);
    }

    // jump when frame window elapses
    if (mState.mFrame >= kJumpWaitFrames) {
      Jump();
    }
  }

  // -- events/move
  void OnMoveX(Input.IStream inputs) {
    var stick = inputs.GetCurrent().mMove;

    var mag = stick.mPosition.x;
    if (mag == 0.0f) {
      return;
    }

    if (mState.Any(State.Type.Airborne)) {
      Drift(mag);
    } else if (mState.Any(State.kDashing)) {
      if (IsHardSwitch(stick, Input.Direction.Horizontal)) {
        Dash(stick.mDirection);
      }
    } else if (mState.Any(State.kRunning)) {
      Run(stick.mDirection);
    } else if (!mState.Any(State.kProhibitsMove)) {
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

    if (mState.Any(State.Type.Falling) && IsHardSwitch(stick, Input.Direction.Down)) {
      FastFall();
    }
  }

  // -- events/run
  void OnDashWait(Input.IStream inputs) {
    if (mState.mFrame < kDashFrames) {
      return;
    }

    // capture primary state
    var state = mState.mType;

    // determine dash direction
    var direction = Input.Direction.Neutral;
    switch (state) {
      case State.Type.DashLeft:
        direction = Input.Direction.Left; break;
      case State.Type.DashRight:
        direction = Input.Direction.Right; break;
    }

    // enter run if dash and current direction are the same
    var stick = inputs.GetCurrent().mMove;
    if (stick.mDirection != direction) {
      // TODO: enter a skid/stop state
      SwitchState(State.Type.Idling);
    } else if (state == State.Type.DashLeft) {
      SwitchState(State.Type.RunLeft);
    } else {
      SwitchState(State.Type.RunRight);
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
    var state = direction.IsLeft() ? State.Type.DashLeft : State.Type.DashRight;

    // ignore repeat dashes, but allow dash back
    if (mState.Is(State.kDashing) && mState.Is(state)) {
      return;
    }

    SwitchState(state);
    mVelocity = new U.Vector2(direction.IsLeft() ? -kDash : kDash, 0.0f);
  }

  void Run(Input.Direction direction) {
    // determine next state
    var next = State.Type.Idling;
    switch (direction) {
      case Input.Direction.Left:
        next = State.Type.RunLeft; break;
      case Input.Direction.Right:
        next = State.Type.RunRight; break;
    }

    // stay in run if it matches the current state
    var curr = mState.mType;
    if (curr == next) {
      mVelocity = new U.Vector2(direction.IsLeft() ? -kRun : kRun, 0.0f);
    } else {
      // TODO: switch to "Stopping" state, Idling when v.x = 0
      SwitchState(State.Type.Idling);
    }
  }

  void Walk(float xMove) {
    // TODO: should SwitchState ignore duplicate states, and also, what is a duplicate?
    if (!mState.Is(State.Type.Walking)) {
      SwitchState(State.Type.Walking);
    }

    mVelocity = new U.Vector2(xMove * kWalk, 0.0f);
  }

  // -- commands/jump
  void StartFullJump() {
    SwitchState(State.Type.Jump1fWait);
  }

  void StartShortJump() {
    SwitchState(State.Type.Jump1sWait);
  }

  void Jump() {
    var isShort = mState.mType == State.Type.Jump1sWait;
    SwitchState(State.Type.Airborne);
    mVelocity = mVelocity.WithY(isShort ? kJumpShort : kJump);
  }

  void Drift(float xMov) {
    mForce.x += xMov * kDrift;
  }

  void Fall() {
    JoinState(State.Type.Falling);
  }

  void FastFall() {
    mVelocity = mVelocity.WithY(-kFastFall);
  }

  public void Land() {
    if (!mState.Any(State.Type.Falling)) {
      return;
    }

    SwitchState(State.Type.Idling);
  }

  void LimitAirSpeed() {
    var v = mVelocity;
    mVelocity = new U.Vector2(U.Mathf.Clamp(v.x, -kMaxAirSpeedX, kMaxAirSpeedX), v.y);
  }

  private void SwitchState(State.Type type) {
    Log.Debug("[Player] State Switch: " + type);
    mState = new State(type);
  }

  private void ReplaceState(State.Type type) {
    Log.Debug("[Player] State Replace: " + type);
    mState.Replace(type);
  }

  private void JoinState(State.Type type) {
    Log.Debug("[Player] State Join: " + type);
    mState.Join(type);
  }

  // -- state --
  sealed class State: IEnumerable<State>, IEquatable<State.Type> {
    // -- state/properties
    public Type mType;
    public int mFrame;
    private State mNext;

    // -- state/types
    public enum Type {
      Idling = 1 << 0,
      Walking = 1 << 1,
      DashLeft = 1 << 2,
      DashRight = 1 << 3,
      RunLeft = 1 << 4,
      RunRight = 1 << 5,
      Jump1fWait = 1 << 6,
      Jump1sWait = 1 << 7,
      Airborne = 1 << 8,
      Falling = 1 << 9
    }

    public const Type kJumpStart = 0
      | Type.Jump1fWait
      | Type.Jump1sWait;

    public const Type kDashing = 0
      | Type.DashLeft
      | Type.DashRight;

    public const Type kRunning = 0
      | Type.RunLeft
      | Type.RunRight;

    public const Type kProhibitsMove = 0
      | kJumpStart;

    public const Type kProhibitsJump1 = 0
      | kJumpStart
      | Type.Airborne;

    // -- state/lifetime
    public State(Type type) {
      mType = type;
      mFrame = 0;
    }

    // -- state/commands
    public void AdvanceFrame() {
      mFrame++;
    }

    public void Replace(Type type) {
      mType = type;
    }

    public void Join(Type type) {
      this.Last().mNext = new State(type);
    }

    // -- state/queries
    public State Find(Type type) {
      return this.First((s) => (s.mType & type) != 0);
    }

    public bool Is(Type type) {
      return this.All((s) => (s.mType & type) != 0);
    }

    public bool Any(Type type) {
      return this.Any((s) => (s.mType & type) != 0);
    }

    // -- state/IEnumerable
    public IEnumerator<State> GetEnumerator() {
      var node = this;
      while (node != null) {
        yield return node;
        node = node.mNext;
      }
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return this.GetEnumerator();
    }

    // -- state/IEquatable
    public bool Equals(Type type) {
      return this.All((s) => s.mType == type);
    }
  }
}
