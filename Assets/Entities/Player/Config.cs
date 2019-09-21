namespace Player {
  public static class Config {
    // -- run/walk-
    public const float Run = 4.0f;
    public const float Walk = 2.0f;

    // -- dash --
    public const int DashFrames = 15;
    public const float DashInitial = 1.5f;
    public const float DashBase = 0.2f;
    public const float DashScale = 0.5f;


    // -- jump --
    public const int JumpWaitFrames = 8;
    public const float Jump = 5.0f;
    public const float JumpShort = 3.0f;
    public const float FastFall = 6.0f;

    // -- airborne --
    public const float Drift = 0.2f;
    public const float MaxAirSpeedX = 3.0f;
  }
}
