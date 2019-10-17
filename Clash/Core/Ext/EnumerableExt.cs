using System.Collections.Generic;

namespace Clash.Ext {
  public static class EnumerableExt {
    public static IEnumerable<int> To(this int from, int to) {
      if (from < to) {
        while (from <= to) {
          yield return from++;
        }
      } else {
        while (from >= to) {
          yield return from--;
        }
      }
    }
  }
}
