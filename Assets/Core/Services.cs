public sealed class Services {
  // -- services --
  private Controls mControls;

  public Controls Controls() {
    if (mControls == null) {
      mControls = new Controls();
    }

    return mControls;
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
