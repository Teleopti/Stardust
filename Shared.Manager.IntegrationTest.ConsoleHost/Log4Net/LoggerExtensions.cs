using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using log4net;

namespace  Manager.IntegrationTest.ConsoleHost.Log4Net
{
	public static class LoggerExtensions
	{
		public static ILog Log<T>(this T thing)
		{
			var log = LogManager.GetLogger(typeof (T));

			return log;
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
				logger.Debug(string.Format("{0}_{1}({2}): {3}",
				                           Path.GetFileName(file),
				                           member,
				                           line,
				                           info));
			}
		}

		public static void InfoWithLineNumber(this ILog logger,
		                                      IEnumerable<string> info,
		                                      [CallerFilePath] string file = "",
		                                      [CallerMemberName] string member = "",
		                                      [CallerLineNumber] int line = 0)
		{
			if (info.Any())
			{
				foreach (var s in info)
				{
					InfoWithLineNumber(logger,
					                   s,
					                   file,
					                   member,
					                   line);
				}
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
				logger.Info(string.Format("{0}_{1}({2}): {3}",
				                          Path.GetFileName(file),
				                          member,
				                          line,
				                          info));
			}
		}
	}
}