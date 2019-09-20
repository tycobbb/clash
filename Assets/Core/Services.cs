public sealed class Services {
  // -- services --
  private IControls mControls;

  public IControls Controls() {
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
