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

    // check state changes
    if (mVelocity.y < 0.0f && mState.Is(State.Type.Airborne)) {
      JoinState(State.Type.Falling);
    }
  }

  public void OnUpdate(IControls controls) {
    // handle button input
    if (controls.GetJumpDown()) {
      OnJumpDown();
    }

    // tick through any state changes
    switch (mState.mType) {
      case State.Type.JumpWait:
        OnJumpWait(controls.GetJump()); break;
    }

    // apply horizontal movement
    OnMoveX(controls.GetMoveX());
  }

  void OnJumpDown() {
    if (!mState.Includes(State.Type.Airborne)) {
      StartJump();
    }
  }

  void OnJumpWait(bool isJumpDown) {
    if (mState.mFrame >= kJumpWaitFrames) {
      Jump(!isJumpDown);
    }
  }

  void OnMoveX(float mag) {
    if (mag == 0.0f) {
      return;
    }

    if (mState.Includes(State.Type.Airborne)) {
      Drift(mag);
    } else if (!mState.Includes(State.Type.JumpWait)) {
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

  void StartJump() {
    SwitchState(State.Type.JumpWait);
    mVelocity = mVelocity.WithY(kJump);
  }

  void Jump(bool isShort) {
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
      Idling,
      Walking,
      JumpWait,
      Airborne,
      Falling,
    }

    // -- state/lifetime
    public State(Type type) {
      mType = type;
      mFrame = 0;
    }

    // -- state/commands
    public void AdvanceFrame() {
      mFrame++;
    }

    public void Join(Type type) {
      this.Last().mNext = new State(type);
    }

    // -- state/queries
    public bool Is(Type type) {
      return this.All((s) => s.mType == type);
    }

    public bool Includes(Type type) {
      return this.Any((s) => s.mType == type);
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
