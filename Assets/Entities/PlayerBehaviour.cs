using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerBehaviour: MonoBehaviour {
  // -- dependencies --
  private Player mPlayer;
  private IControls mControls;

  // -- physics --

  // -- lifecycle --
  void Awake() {
    mControls = Services.Root.Controls();
    mPlayer = new Player();
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

    // sync body to entity and run update
    mPlayer.OnPreUpdate(body.velocity);
    mPlayer.OnUpdate(mControls);

    // sync entity back to body
    var newV = mPlayer.mVelocity;
    if (!body.velocity.Equals(newV)) {
      body.velocity = newV;
    }

    var newF = mPlayer.mForce;
    if (!newF.Equals(Vector2.zero)) {
      body.AddForce(newF, ForceMode2D.Impulse);
    }

    // run post-simulation
    mPlayer.OnPostSimulation(body.velocity);

    newV = mPlayer.mVelocity;
    if (!body.velocity.Equals(newV)) {
      body.velocity = newV;
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
