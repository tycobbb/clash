using UnityEditor;
using UnityEngine;
using C = Clash;

public class GameController: MonoBehaviour {
  // -- properties --
  private readonly bool devMode = true;

  // -- dependencies --
  private C.Input.IMutableStream inputs;

  // -- lifecycle --
  public void Awake() {
    C.Log.Level = C.LogLevel.Debug;
    C.Log.LogFn = Debug.Log;
    C.Log.LogErrFn = Debug.LogError;

    var services = Services.Root;
    inputs = services.Inputs();
  }

  public void Start() {
    if (devMode) {
      EnableDevMode();
    }
  }

  public void FixedUpdate() {
    inputs.OnUpdate(Time.fixedUnscaledTime);
  }

  // -- commands --
  private void EnableDevMode() {
    C.Log.Info("[Game] Dev Mode Enabled!");
    EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
  }
}
