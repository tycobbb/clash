using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerBehaviour: MonoBehaviour {
  // -- dependencies --
  private Player mPlayer;
  private Controls mControls;

  // -- physics --

  // -- lifecycle --
  void Awake() {
    mPlayer = new Player();
    mControls = Services.Root.Controls();
  }

  void Start() {
    // set initial state
    var nContacts = Body().GetContacts(new Collider2D[0]);
    mPlayer.OnStart(nContacts == 0);

    // sync constants
    Body().gravityScale = Player.kGravity;
  }

  void FixedUpdate() {
    var body = Body();

    // sync state pre-update
    mPlayer.Sync(body.velocity);

    // fire events
    if (mControls.IsJumpDown()) {
      mPlayer.OnJumpDown();
    }

    mPlayer.OnDefault(mControls.LeftStickX());

    // update state
    var newV = mPlayer.mVelocity;
    if (!body.velocity.Equals(newV)) {
      body.velocity = newV;
    }

    var newF = mPlayer.mForce;
    if (!newF.Equals(Vector2.zero)) {
      body.AddForce(newF, ForceMode2D.Impulse);
    }
  }

  void OnCollisionEnter2D(Collision2D other) {
    // TODO: land conditionally, not on every collision
    mPlayer.Land();
  }

  // -- queries --
  private Rigidbody2D Body() {
    var body = GetComponent<Rigidbody2D>();
    if (body == null) {
      Debug.LogError("[Player] missing body!");
    }

    return body;
  }
}
