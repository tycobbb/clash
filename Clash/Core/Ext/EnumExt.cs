using System;

namespace Clash.Ext {
  public static class EnumExt {
    public static bool Has(this Enum flags, Enum flag) {
      return flags.HasFlag(flag);
    }

    public static bool All(this Enum flags, Enum flag) {
      return flags.HasFlag(flag);
    }

    public static bool Any(this Enum flags, Enum flag) {
      return ((int)(object)flags & (int)(object)flag) != 0;
    }
  }
}
