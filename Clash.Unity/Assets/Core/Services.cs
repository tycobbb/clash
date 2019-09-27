using C = Clash;

public sealed class Services {
  // -- services --
  private C.Input.IMutableStream inputs;

  public C.Input.IMutableStream Inputs() {
    if (inputs == null) {
      inputs = new C.Input.Stream(new Input.Source());
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
