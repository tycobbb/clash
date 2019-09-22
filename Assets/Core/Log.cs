using System;

public static class Log {
  // -- dependencies --
  public static Action<string> LogFn;
  public static Action<string> LogErrFn;

  // -- properties --
  public static LogLevel Level;

  // -- impls --
  public static void Error(string message) {
    LogMessage(LogLevel.Error, message);
  }
  public static void Info(string message) {
    LogMessage(LogLevel.Info, message);
  }

  public static void Debug(string message) {
    LogMessage(LogLevel.Debug, message);
  }

  public static void Verbose(string message) {
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
public enum LogLevel {
  None,
  Error,
  Info,
  Debug,
  Verbose
}
