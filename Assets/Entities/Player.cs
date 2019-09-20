using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class Player {
  // -- core --
  public Vector2 mVelocity = Vector2.zero;
  public Vector2 mForce = Vector2.zero;

  // -- constants --
  public const float kGravity = 1.0f;
  private const int kJumpWaitFrames = 8;
  private const float kJump = 5.0f;
  private const float kJumpShort = 3.0f;
  private const float kWalk = 3.0f;
  private const float kDrift = 0.2f;
  private const float kMaxAirSpeed = 3.0f;

  // -- state-machine --
  private State mState;

  // -- events --
  public void OnStart(bool isAirborne) {
    var state = isAirborne ? State.Type.Airborne : State.Type.Idling;
    mState = new State(state);
  }

  public void OnPreUpdate(Vector2 v) {
    mState.AdvanceFrame();

    // sync body data
    mForce = Vector2.zero;
    mVelocity = new Vector2(v.x, v.y);
  }

  public void OnUpdate(IControls controls) {
    // check state changes
    if (mVelocity.y < 0.0f && mState.Is(State.Type.Airborne)) {
      JoinState(State.Type.Falling);
    }

    // handle button input
    if (controls.GetJumpDown()) {
      OnFullJumpDown();
    } else if (controls.GetJump2Down()) {
      OnShortJumpDown();
    }

    // tick through any state changes
    if (mState.Includes(State.kJumpStart)) {
      OnJumpWait(controls.GetJump());
    }

    // apply horizontal movement
    OnMoveX(controls.GetMoveX());
  }

  void OnFullJumpDown() {
    if (!mState.Includes(State.kProhibitsJump1)) {
      StartFullJump();
    }
  }

  void OnShortJumpDown() {
    if (!mState.Includes(State.kProhibitsJump1)) {
      StartShortJump();
    }
  }

  void OnJumpWait(bool isJumpDown) {
    // switch to short jump if jump is released within frame window
    if (mState.mType == State.Type.Jump1fWait && !isJumpDown) {
      ReplaceState(State.Type.Jump1sWait);
    }

    if (mState.mFrame >= kJumpWaitFrames) {
      Jump();
    }
  }

  void OnMoveX(float mag) {
    if (mag == 0.0f) {
      return;
    }

    if (mState.Includes(State.Type.Airborne)) {
      Drift(mag);
    } else if (!mState.Includes(State.Type.Jump1fWait | State.Type.Jump1sWait)) {
      Move(mag);
    }
  }

  public void OnPostSimulation(Vector2 v) {
    mVelocity = v;

    if (mState.Includes(State.Type.Airborne)) {
      LimitAirSpeed();
    }
  }

  // -- commands --
  void Move(float xMove) {
    // TODO: should SwitchState ignore duplicate states, and also, what is a duplicate?
    if (!mState.Is(State.Type.Walking)) {
      SwitchState(State.Type.Walking);
    }

    mVelocity = new Vector2(xMove * kWalk, 0.0f);
  }

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

  public void Land() {
    if (!mState.Includes(State.Type.Falling)) {
      return;
    }

    SwitchState(State.Type.Idling);
  }

  void LimitAirSpeed() {
    var v = mVelocity;
    mVelocity = new Vector2(Math.Min(Math.Max(v.x, -kMaxAirSpeed), kMaxAirSpeed), v.y);
  }

  private void SwitchState(State.Type type) {
    Debug.Log("[Player] state Switch: " + type);
    mState = new State(type);
  }

  private void ReplaceState(State.Type type) {
    Debug.Log("[Player] state Replace: " + type);
    mState.Replace(type);
  }

  private void JoinState(State.Type type) {
    Debug.Log("[Player] state Join: " + type);
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
      Jump1fWait = 1 << 2,
      Jump1sWait = 1 << 3,
      Airborne = 1 << 4,
      Falling = 1 << 5
    }

    public const Type kJumpStart = 0
      | Type.Jump1fWait
      | Type.Jump1sWait;

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
    public bool Is(Type type) {
      return this.All((s) => (s.mType & type) != 0);
    }

    public bool Includes(Type type) {
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
