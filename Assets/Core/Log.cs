using System;

internal static class Log {
  // -- dependencies --
  internal static Action<string> LogFn;
  internal static Action<string> LogErrFn;

  // -- properties --
  internal static Level sLevel;

  // -- impls --
  internal static void Error(string message) {
    LogMessage(Level.Error, message);
  }
  internal static void Info(string message) {
    LogMessage(Level.Info, message);
  }

  internal static void Debug(string message) {
    LogMessage(Level.Debug, message);
  }

  internal static void Verbose(string message) {
    LogMessage(Level.Verbose, message);
  }

  private static void LogMessage(Level level, string message) {
    if (level <= sLevel) {
      message = "[" + Prefix(level) + "] " + message;
      if (level != Level.Error) {
        LogFn(message);
      } else {
        LogErrFn(message);
      }
    }
  }

  private static string Prefix(Level level) {
    switch (level) {
      case Level.Error:
        return "E";
      case Level.Info:
        return "I";
      case Level.Debug:
        return "D";
      case Level.Verbose:
        return "V";
      default:
        return "";
    }
  }

  // -- types --
  internal enum Level {
    None,
    Error,
    Info,
    Debug,
    Verbose
  }
}
