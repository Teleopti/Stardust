using System;
using System.IO;
using System.Runtime.CompilerServices;
using log4net;

namespace Stardust.Node.Extensions
{
	public static class LoggerExtensions
	{
		public static ILog Log<T>(this T thing)
		{
			var log = LogManager.GetLogger(typeof (T));

			return log;
		}

		private static void ValidateArgument(this ILog logger)
		{
			if (logger == null)
			{
				throw new ArgumentNullException(nameof(logger));
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

		public static void ErrorWithLineNumber(this ILog logger,
		                                       string info,
		                                       Exception exception = null,
		                                       [CallerFilePath] string file = "",
		                                       [CallerMemberName] string member = "",
		                                       [CallerLineNumber] int line = 0)
		{
			if (!logger.IsErrorEnabled) return;
			ValidateArgument(logger);

			logger.Error($"{Path.GetFileName(file)}_{member}({line}): {info}",
			             exception);
		}

		public static void FatalWithLineNumber(this ILog logger,
		                                       string info,
		                                       Exception exception = null,
		                                       [CallerFilePath] string file = "",
		                                       [CallerMemberName] string member = "",
		                                       [CallerLineNumber] int line = 0)
		{
			if (!logger.IsFatalEnabled) return;
			ValidateArgument(logger);

			logger.Fatal($"{Path.GetFileName(file)}_{member}({line}): {info}",
			             exception);
		}

		public static void WarningWithLineNumber(this ILog logger,
		                                         string info,
		                                         [CallerFilePath] string file = "",
		                                         [CallerMemberName] string member = "",
		                                         [CallerLineNumber] int line = 0)
		{
			if (!logger.IsWarnEnabled) return;
			ValidateArgument(logger);

			logger.Warn($"{Path.GetFileName(file)}_{member}({line}): {info}");
		}

		public static void DebugWithLineNumber(this ILog logger,
		                                       string info,
		                                       [CallerFilePath] string file = "",
		                                       [CallerMemberName] string member = "",
		                                       [CallerLineNumber] int line = 0)
		{
			if (!logger.IsDebugEnabled) return;
			ValidateArgument(logger);

			logger.Debug($"{Path.GetFileName(file)}_{member}({line}): {info}");
		}

		public static void InfoWithLineNumber(this ILog logger,
		                                      string info,
		                                      [CallerFilePath] string file = "",
		                                      [CallerMemberName] string member = "",
		                                      [CallerLineNumber] int line = 0)
		{
			if (!logger.IsInfoEnabled) return;
			ValidateArgument(logger);

			logger.Info($"{Path.GetFileName(file)}_{member}({line}): {info}");
		}
	}
}