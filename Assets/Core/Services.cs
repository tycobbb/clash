public sealed class Services {
  // -- services --
  private Input.IMutableStream inputs;

  public Input.IMutableStream Inputs() {
    if (inputs == null) {
      inputs = new Input.Stream();
    }

    return inputs;
  }

  // -- singleton (TODO: inject this) --
  private static Services mRoot = null;

  public static Services Root {
    get {
      if (mRoot == null) {
        mRoot = new Services();
      }

      return mRoot;
    }
  }
}
