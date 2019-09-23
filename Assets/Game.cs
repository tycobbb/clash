using UnityEngine;

public class Game: MonoBehaviour {
  // -- properties --
  private readonly bool devMode = true;

  // -- dependencies --
  private Input.IMutableStream inputs;
  private EntityRepo entities;

  // -- lifecycle --
  public void Awake() {
    Log.Level = LogLevel.Verbose;
    Log.LogFn = Debug.Log;
    Log.LogErrFn = Debug.LogError;

    var services = Services.Root;
    inputs = services.Inputs();
    entities = services.Entities();
  }

  public void Start() {
    if (devMode) {
      Log.Info("[Game] Dev Mode Enabled!");

      var visible = entities.FindVisible();
      foreach (var entity in visible) {
        var renderer = entity.GetComponent<MeshRenderer>();
        renderer.material = GetDebugMaterial();
      }
    }
  }

  public void FixedUpdate() {
    inputs.OnUpdate();
  }

  // -- queries --
  private Material GetDebugMaterial() {
    return GetComponent<MeshRenderer>().material;
  }
}
