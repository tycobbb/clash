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
  private static readonly float mGravity = 1.0f;
  private static readonly float mJump = 5.0f;
  private static readonly float mDrift = 2.0f;

  // -- state-machine --
  private State mState;

  // -- lifetime --
  public Player() {
    mState = new State(State.Type.Idling);
  }

  // -- events --
  public void OnJumpDown(float xAxis) {
    if (!mState.Includes(State.Type.Jumping)) {
      Jump();
    }

    if (mState.Includes(State.Type.Jumping)) {
      Debug.Log("x: " + xAxis);
      JumpDrift(xAxis);
    }
  }

  // -- commands --
  public void Sync(Vector2 v) {
    // sync body data
    mState.AdvanceFrame();
    mForce = Vector2.zero;
    mVelocity = new Vector2(v.x, v.y);

    // detect state changes
    if (mVelocity.y < 0.0f && mState.Is(State.Type.Jumping)) {
      JoinState(State.Type.Falling);
    }
  }

  public void Jump() {
    SwitchState(State.Type.Jumping);
    mVelocity = mVelocity.WithY(mJump);
  }

  public void JumpDrift(float xAxis) {
    mForce.x += xAxis < 0.0f ? -mDrift : mDrift;
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

  // -- queries --
  public Vector2 Velocity() {
    return mVelocity;
  }

  public float Gravity() {
    return mGravity;
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
      Jumping = 1 << 1,
      Falling = 1 << 2,
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
