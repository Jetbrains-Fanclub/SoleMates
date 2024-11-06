using Hangfire.Console;
using Hangfire.Server;
using Microsoft.Extensions.Logging;

namespace Website.Extensions.Fetch.Services;
public class LoggerService {
  private readonly ILogger<LoggerService> _logger;
  private readonly PerformContext? _context;

  public LoggerService(ILogger<LoggerService> logger, PerformContext? context = null) {
    _logger = logger;
    _context = context;
  }

  public void Log(LogLevel level, string format, params object[] args) {
    _logger.Log(level, format, args);
    _context?.WriteLine(GetTextColor(level), format, args);
  }

  public void Log(LogLevel level, Exception exception, string message, params object[] args) {
    _logger.Log(level, exception, message);
    _context?.SetTextColor(GetTextColor(level));
    _context?.WriteLine($"{{Message}} {{StackTrace}} {message}", exception.Message, exception.StackTrace, args);
  }

  private static ConsoleTextColor GetTextColor(LogLevel level) {
    switch (level) {
      case LogLevel.Error:
      case LogLevel.Critical:
        return ConsoleTextColor.Red;
      case LogLevel.Warning:
        return ConsoleTextColor.Yellow;
      default:
        return ConsoleTextColor.White;
    }
  }
}
