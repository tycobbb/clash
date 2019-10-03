using UnityEngine;
using C = Clash;
using Clash.Player;
using K = Clash.Player.Config;

public sealed class PlayerController: MonoBehaviour {
  // -- entities --
  private Player player;

  // -- dependencies --
  private C.Input.IMutableStream inputs;

  // -- lifecycle --
  public void Awake() {
    // set dependencies
    inputs = Services.Root.Inputs();
  }

  public void Start() {
    // set constants
    var body = Body();
    body.freezeRotation = true;

    // size colliders
    var collider = Collider();
    collider.offset = K.Offset.ToNative();
    collider.size = K.Size.ToNative();

    var pushbox = Pushbox();
    pushbox.SetFrame(K.Offset.ToNative(), K.PushboxSize.ToNative());

    var hurtbox = Hurtbox();
    hurtbox.SetFrame(K.Offset.ToNative(), K.HurtboxSize.ToNative());

    // set initial state
    var nContacts = body.GetContacts(new Collider2D[0]);
    player = new Player(isOnGround: nContacts != 0);

    SyncEntityState();
  }

  public void FixedUpdate() {
    var body = Body();
    body.sharedMaterial.friction = K.Friction;

    // sync body to entity and run update
    player.OnPreUpdate(body.velocity.ToDomain());
    player.OnUpdate(inputs);
    SyncEntityState();

    // run post-simulation
    player.OnPostSimulation(body.velocity.ToDomain());
    SyncEntityState();
  }

  public void OnCollisionEnter2D(Collision2D _) {
    // TODO: pass collision information into domain
    player.OnCollide();
  }

  // -- commands --
  private void SyncEntityState() {
    var body = Body();

    // sync physics constants
    body.gravityScale = player.Gravity;

    // sync physics state
    var newV = player.Velocity.ToNative();
    if (!body.velocity.Equals(newV)) {
      body.velocity = newV;
    }

    var newF = player.Force.ToNative();
    if (!newF.Equals(Vector2.zero)) {
      body.AddForce(newF, ForceMode2D.Impulse);
    }

    // apply visual transforms
    transform.localEulerAngles = Vector3.up * (player.IsFacingLeft ? 180.0f : 0.0f);
  }

  // -- queries --
  private Rigidbody2D Body() {
    var body = GetComponent<Rigidbody2D>();
    if (body == null) {
      C.Log.Error("[Player] missing body!");
    }

    return body;
  }

  private BoxCollider2D Collider() {
    var collider = GetComponentInChildren<BoxCollider2D>();
    if (collider == null) {
      C.Log.Error("[Player] missing collider!");
    }

    return collider;
  }

  private Pushbox2D Pushbox() {
    var pushbox = GetComponentInChildren<Pushbox2D>();
    if (pushbox == null) {
      C.Log.Error("[Player] missing pushbox!");
    }

    return pushbox;
  }

  private Hurtbox2D Hurtbox() {
    var hurtbox = GetComponentInChildren<Hurtbox2D>();
    if (hurtbox == null) {
      C.Log.Error("[Player] missing hurtbox!");
    }

    return hurtbox;
  }
}
