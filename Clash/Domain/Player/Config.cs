using Clash.Maths;

namespace Clash.Player {
  public static class Config {
    public const float Gravity = 1.0f;
    public const float Friction = 0.65f;

    // -- run/walk-
    public const float Run = 6.0f;
    public const float Walk = 2.0f;
    public const int RunPivotFrames = 30;

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
    public const float MaxAirSpeedX = 6.0f;

    // -- air dodge --
    public const int AirDodgeFrames = 49;
    public const float AirDodge = 5.0f;

    // -- size / hit detection --
    public static readonly Vec Size = new Vec(1.0f, 2.0f);
    public static readonly Vec HurtboxSize = new Vec(1.0f, 0.9f);
  }
}
