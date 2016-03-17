using System;
using System.IO;
using System.Runtime.CompilerServices;
using log4net;

namespace Stardust.Manager.Extensions
{
	public static class LoggerExtensions
	{
		public static ILog Log<T>(this T thing)
		{
			var log = LogManager.GetLogger(typeof(T));

			return log;
		}

		private static void ValidateArgument(this ILog logger)
		{
			if (logger == null)
			{
				throw new ArgumentNullException("logger");
			}
		}

		public static void ErrorWithLineNumber(this ILog logger,
		                                          string info,
		                                          Exception exception = null,
		                                          [CallerFilePath] string file = "",
		                                          [CallerMemberName] string member = "",
		                                          [CallerLineNumber] int line = 0)
		{			

			if (logger.IsErrorEnabled)
			{
				ValidateArgument(logger);

				logger.Error(string.Format("{0}_{1}({2}): {3}",
				                           Path.GetFileName(file),
				                           member,
				                           line,
				                           info),
				             exception);
			}
		}

		public static void FatalWithLineNumber(this ILog logger,
		                                          string info,
		                                          Exception exception = null,
		                                          [CallerFilePath] string file = "",
		                                          [CallerMemberName] string member = "",
		                                          [CallerLineNumber] int line = 0)
		{
			if (logger.IsFatalEnabled)
			{
				ValidateArgument(logger);

				logger.Fatal(string.Format("{0}_{1}({2}): {3}",
				                           Path.GetFileName(file),
				                           member,
				                           line,
				                           info),
				             exception);
			}
		}

		public static void WarningWithLineNumber(this ILog logger,
		                                            string info,
		                                            [CallerFilePath] string file = "",
		                                            [CallerMemberName] string member = "",
		                                            [CallerLineNumber] int line = 0)
		{
			if (logger.IsWarnEnabled)
			{
				ValidateArgument(logger);

				logger.Warn(string.Format("{0}_{1}({2}): {3}",
				                          Path.GetFileName(file),
				                          member,
				                          line,
				                          info));
			}
		}

		public static void DebugWithLineNumber(this ILog logger,
		                                          string info,
		                                          [CallerFilePath] string file = "",
		                                          [CallerMemberName] string member = "",
		                                          [CallerLineNumber] int line = 0)
		{
			if (logger.IsDebugEnabled)
			{
				ValidateArgument(logger);

				logger.Debug(string.Format("{0}_{1}({2}): {3}",
				                           Path.GetFileName(file),
				                           member,
				                           line,
				                           info));
			}
		}

		public static void InfoWithLineNumber(this ILog logger,
		                                         string info,
		                                         [CallerFilePath] string file = "",
		                                         [CallerMemberName] string member = "",
		                                         [CallerLineNumber] int line = 0)
		{
			if (logger.IsInfoEnabled)
			{
				ValidateArgument(logger);

				logger.Info(string.Format("{0}_{1}({2}): {3}",
				                          Path.GetFileName(file),
				                          member,
				                          line,
				                          info));
			}
		}
	}
}