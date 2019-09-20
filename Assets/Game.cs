using UnityEngine;

public class Game: MonoBehaviour {
  // -- properties --
  private readonly bool mDevMode = true;

  // -- dependencies --
  private Input.IMutableStream mInputs;
  private EntityRepo mEntities;

  // -- lifecycle --
  internal void Awake() {
    Log.sLevel = Log.Level.Debug;
    Log.LogFn = Debug.Log;
    Log.LogErrFn = Debug.LogError;

    mEntities = new EntityRepo();
    mInputs = Services.Root.Inputs();
  }

  internal void Start() {
    if (mDevMode) {
      Log.Info("[Game] Dev Mode Enabled!");

      var visible = mEntities.FindVisible();
      foreach (var entity in visible) {
        var renderer = entity.GetComponent<MeshRenderer>();
        renderer.material = GetDebugMaterial();
      }
    }
  }

  internal void FixedUpdate() {
    mInputs.OnUpdate();
  }

  // -- queries --
  private Material GetDebugMaterial() {
    return GetComponent<MeshRenderer>().material;
  }
}
