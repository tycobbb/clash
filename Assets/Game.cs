using UnityEngine;

public class Game: MonoBehaviour {
  // -- properties --
  private readonly bool mDevMode = true;

  // -- dependencies --
  private EntityRepo mEntities;

  // -- lifecycle --
  void Awake() {
    mEntities = new EntityRepo();
  }

  void Start() {
    if (mDevMode) {
      Debug.Log("[Game] dev mode enabled");

      var visible = mEntities.FindVisible();
      foreach (var entity in visible) {
        var renderer = entity.GetComponent<MeshRenderer>();
        renderer.material = GetDebugMaterial();
      }
    }
  }

  // -- queries --
  private Material GetDebugMaterial() {
    return GetComponent<MeshRenderer>().material;
  }
}
