using UnityEngine;
using C = Clash;

public class GameController: MonoBehaviour {
  // -- properties --
  private readonly bool devMode = true;

  // -- dependencies --
  private C.Input.IMutableStream inputs;
  private EntityRepo entities;

  // -- lifecycle --
  public void Awake() {
    C.Log.Level = C.LogLevel.Verbose;
    C.Log.LogFn = Debug.Log;
    C.Log.LogErrFn = Debug.LogError;

    var services = Services.Root;
    inputs = services.Inputs();
    entities = services.Entities();
  }

  public void Start() {
    if (devMode) {
      EnableDevMode();
    }
  }

  public void FixedUpdate() {
    inputs.OnUpdate(Time.fixedUnscaledTime);
  }

  // -- queries --
  private Material GetDebugMaterial() {
    return GetComponent<MeshRenderer>().material;
  }

  // -- commands --
  private void EnableDevMode() {
    C.Log.Info("[Game] Dev Mode Enabled!");

    var visible = entities.FindVisible();
    foreach (var entity in visible) {
      var renderer = entity.GetComponent<MeshRenderer>();
      renderer.material = GetDebugMaterial();
    }
  }
}
