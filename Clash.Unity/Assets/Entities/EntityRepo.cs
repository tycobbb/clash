using System.Linq;
using UnityEngine;

public class EntityRepo {
  // -- queries --
  public GameObject[] FindVisible() {
    return Object
      .FindObjectOfType<Camera>()
      .GetComponentsInChildren<Transform>()
      .Select((t) => t.gameObject)
      .Where((o) => o.GetComponent<Camera>() == null)
      .ToArray();
  }
}
