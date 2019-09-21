using UnityEngine;

namespace Player {
  using K = Config;

  public sealed class Controller: MonoBehaviour {
    // -- dependencies --
    private Player player;
    private Input.IMutableStream inputs;

    // -- lifecycle --
    public void Awake() {
      player = new Player();
      inputs = Services.Root.Inputs();
    }

    public void Start() {
      // set constants
      Body().freezeRotation = true;
      Body().gravityScale = K.Gravity;
      Body().sharedMaterial.friction = K.Friction;

      // set initial state
      var nContacts = Body().GetContacts(new Collider2D[0]);
      player.OnStart(nContacts == 0);
    }

    public void FixedUpdate() {
      var body = Body();

      // sync body to entity and run update
      player.OnPreUpdate(body.velocity);
      player.OnUpdate(inputs);

      // sync entity back to body
      var newV = player.Velocity;
      if (!body.velocity.Equals(newV)) {
        body.velocity = newV;
      }

      var newF = player.Force;
      if (!newF.Equals(Vector2.zero)) {
        body.AddForce(newF, ForceMode2D.Impulse);
      }

      // run post-simulation
      player.OnPostSimulation(body.velocity);

      newV = player.Velocity;
      if (!body.velocity.Equals(newV)) {
        body.velocity = newV;
      }
    }

    public void OnCollisionEnter2D(Collision2D _) {
      // TODO: land conditionally, not on every collision
      player.Land();
    }

    // -- queries --
    private Rigidbody2D Body() {
      var body = GetComponent<Rigidbody2D>();
      if (body == null) {
        Log.Error("[Player] missing body!");
      }

      return body;
    }
  }
}
