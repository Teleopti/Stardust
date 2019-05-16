using System;
using System.IO;
using System.Runtime.CompilerServices;
//using log4net;
using Microsoft.Extensions.Logging;

namespace Stardust.Core.Node.Extensions
{
	public static class LoggerExtensions
	{
		public static ILogger Log<T>(this T thing)
		{
			var log = new LoggerFactory().CreateLogger<T>();
			return log;
		}

		private static void ValidateArgument(this ILogger logger)
		{
			if (logger == null)
			{
				throw new ArgumentNullException("logger");
			}
		}
		
		public static string GetFormattedLogMessage(
			string info,
			[CallerFilePath] string file = "",
			[CallerMemberName] string member = "",
			[CallerLineNumber] int line = 0)
		{
			return  $"{Path.GetFileName(file)}_{member}({line}): {info}";
		}

		public static void ErrorWithLineNumber(this ILogger logger,
		                                       string info,
		                                       Exception exception = null,
		                                       [CallerFilePath] string file = "",
		                                       [CallerMemberName] string member = "",
		                                       [CallerLineNumber] int line = 0)
		{
			if (!logger.IsEnabled(LogLevel.Error)) return;
			ValidateArgument(logger);

			logger.LogError($"{Path.GetFileName(file)}_{member}({line}): {info}",
			             exception);
		}

		public static void FatalWithLineNumber(this ILogger logger,
		                                       string info,
		                                       Exception exception = null,
		                                       [CallerFilePath] string file = "",
		                                       [CallerMemberName] string member = "",
		                                       [CallerLineNumber] int line = 0)
		{
			if (!logger.IsEnabled(LogLevel.Critical)) return;
			ValidateArgument(logger);

			logger.LogCritical($"{Path.GetFileName(file)}_{member}({line}): {info}",
			             exception);
		}

		public static void WarningWithLineNumber(this ILogger logger,
		                                         string info,
		                                         [CallerFilePath] string file = "",
		                                         [CallerMemberName] string member = "",
		                                         [CallerLineNumber] int line = 0)
		{
			if (!logger.IsEnabled(LogLevel.Warning)) return;
			ValidateArgument(logger);

			logger.LogWarning($"{Path.GetFileName(file)}_{member}({line}): {info}");
		}

		public static void DebugWithLineNumber(this ILogger logger,
		                                       string info,
		                                       [CallerFilePath] string file = "",
		                                       [CallerMemberName] string member = "",
		                                       [CallerLineNumber] int line = 0)
		{
			if (!logger.IsEnabled(LogLevel.Debug)) return;
			ValidateArgument(logger);

			logger.LogDebug($"{Path.GetFileName(file)}_{member}({line}): {info}");
		}

		public static void InfoWithLineNumber(this ILogger logger,
		                                      string info,
		                                      [CallerFilePath] string file = "",
		                                      [CallerMemberName] string member = "",
		                                      [CallerLineNumber] int line = 0)
		{
			if (!logger.IsEnabled(LogLevel.Information)) return;
			ValidateArgument(logger);

			logger.LogInformation($"{Path.GetFileName(file)}_{member}({line}): {info}");
		}
	}
}