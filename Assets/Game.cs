using UnityEngine;

public class Game: MonoBehaviour {
  // -- properties --
  private readonly bool mDevMode = true;

  // -- dependencies --
  private Input.IMutableStream mInputs;
  private EntityRepo mEntities;

  // -- lifecycle --
  void Awake() {
    mEntities = new EntityRepo();
    mInputs = Services.Root.Inputs();
  }

  void Start() {
    if (mDevMode) {
      Debug.Log("[Game] Dev Mode Enabled!");

      var visible = mEntities.FindVisible();
      foreach (var entity in visible) {
        var renderer = entity.GetComponent<MeshRenderer>();
        renderer.material = GetDebugMaterial();
      }
    }
  }

  void FixedUpdate() {
    mInputs.OnUpdate();
  }

  // -- queries --
  private Material GetDebugMaterial() {
    return GetComponent<MeshRenderer>().material;
  }
}
