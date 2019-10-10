using Clash.Maths;

namespace Clash.Player {
  public static class Config {
    public const float GravityOn = 1.0f;
    public const float GravityOff = 0.0f;
    public const float Friction = 4.0f;

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
    public const int JumpWaitFrames = 4;
    public const float Jump = 5.0f;
    public const float JumpShort = 3.0f;
    public const float FastFall = 6.0f;

    // -- airborne --
    public const float Drift = 0.2f;
    public const float MaxAirSpeedX = 6.0f;

    // -- air dodge --
    public const float AirDodge = 12.0f;
    public const float AirDodgeDecay = 0.9f;

    // -- size / hit detection --
    public static readonly Vec Offset = new Vec(0.1f, 0.0f);
    public static readonly Vec Size = new Vec(1.0f, 1.6f);
    public static readonly Vec PushboxSize = new Vec(1.0f, 1.6f);
    public static readonly Vec HurtboxSize = new Vec(1.0f, 1.4f);
  }
}
