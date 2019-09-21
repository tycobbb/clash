using UnityEngine;

public class Game: MonoBehaviour {
  // -- properties --
  private readonly bool devMode = true;

  // -- dependencies --
  private Input.IMutableStream inputs;
  private EntityRepo entities;

  // -- lifecycle --
  internal void Awake() {
    Log.Level = LogLevel.Debug;
    Log.LogFn = Debug.Log;
    Log.LogErrFn = Debug.LogError;

    entities = new EntityRepo();
    inputs = Services.Root.Inputs();
  }

  internal void Start() {
    if (devMode) {
      Log.Info("[Game] Dev Mode Enabled!");

      var visible = entities.FindVisible();
      foreach (var entity in visible) {
        var renderer = entity.GetComponent<MeshRenderer>();
        renderer.material = GetDebugMaterial();
      }
    }
  }

  internal void FixedUpdate() {
    inputs.OnUpdate();
  }

  // -- queries --
  private Material GetDebugMaterial() {
    return GetComponent<MeshRenderer>().material;
  }
}
