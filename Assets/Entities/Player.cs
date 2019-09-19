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
  private const float kJump = 5.0f;
  private const float kWalk = 3.0f;
  private const float kDrift = 2.0f;

  // -- state-machine --
  private State mState;

  // -- events --
  public void OnStart(bool isAirborne) {
    var state = isAirborne ? State.Type.Airborne : State.Type.Idling;
    mState = new State(state);
  }

  public void OnJumpDown() {
    if (!mState.Includes(State.Type.Airborne)) {
      Jump();
    }
  }

  public void OnDefault(float xAxis) {
    if (xAxis == 0.0f) {
      return;
    } else if (mState.Includes(State.Type.Airborne)) {
      JumpDrift(xAxis);
    } else {
      Move(xAxis);
    }
  }

  // -- commands --
  public void Sync(Vector2 v) {
    // sync body data
    mState.AdvanceFrame();
    mForce = Vector2.zero;
    mVelocity = new Vector2(v.x, v.y);

    // detect state changes
    if (mVelocity.y < 0.0f && mState.Is(State.Type.Airborne)) {
      JoinState(State.Type.Falling);
    }
  }

  void Move(float xAxis) {
    // TODO: should SwitchState ignore duplicate states, and also, what is a duplicate?
    if (!mState.Is(State.Type.Walking)) {
      SwitchState(State.Type.Walking);
    }

    mVelocity = new Vector2(xAxis * kWalk, 0.0f);
  }

  void Jump() {
    SwitchState(State.Type.Airborne);
    mVelocity = mVelocity.WithY(kJump);
  }

  void JumpDrift(float xAxis) {
    mForce.x += xAxis * kDrift;
  }

  public void Land() {
    if (!mState.Includes(State.Type.Falling)) {
      return;
    }

    SwitchState(State.Type.Idling);
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
