using System;

internal static class Log {
  // -- dependencies --
  internal static Action<string> LogFn;
  internal static Action<string> LogErrFn;

  // -- properties --
  internal static LogLevel Level;

  // -- impls --
  internal static void Error(string message) {
    LogMessage(LogLevel.Error, message);
  }
  internal static void Info(string message) {
    LogMessage(LogLevel.Info, message);
  }

  internal static void Debug(string message) {
    LogMessage(LogLevel.Debug, message);
  }

  internal static void Verbose(string message) {
    LogMessage(LogLevel.Verbose, message);
  }

  private static void LogMessage(LogLevel level, string message) {
    if (level <= Level) {
      message = "[" + Prefix(level) + "] " + message;
      if (level != LogLevel.Error) {
        LogFn(message);
      } else {
        LogErrFn(message);
      }
    }
  }

  private static string Prefix(LogLevel level) {
    switch (level) {
      case LogLevel.Error:
        return "E";
      case LogLevel.Info:
        return "I";
      case LogLevel.Debug:
        return "D";
      case LogLevel.Verbose:
        return "V";
      default:
        return "";
    }
  }
}

// -- types --
internal enum LogLevel {
  None,
  Error,
  Info,
  Debug,
  Verbose
}
