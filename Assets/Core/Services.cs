public sealed class Services {
  // -- services --
  private Input.IMutableStream mInputs;

  public Input.IMutableStream Inputs() {
    if (mInputs == null) {
      mInputs = new Input.Stream();
    }

    return mInputs;
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
