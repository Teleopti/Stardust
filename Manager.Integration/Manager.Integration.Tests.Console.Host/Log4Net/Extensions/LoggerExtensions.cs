using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using log4net;

namespace Manager.IntegrationTest.Console.Host.Log4Net.Extensions
{
	public static class LoggerExtensions
	{
		public static void LogErrorWithLineNumber(this ILog logger,
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

		public static void LogFatalWithLineNumber(this ILog logger,
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

		public static void LogWarningWithLineNumber(this ILog logger,
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

		public static void LogDebugWithLineNumber(this ILog logger,
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

		public static void LogInfoWithLineNumber(this ILog logger,
		                                         IEnumerable<string> info,
		                                         [CallerFilePath] string file = "",
		                                         [CallerMemberName] string member = "",
		                                         [CallerLineNumber] int line = 0)
		{
			if (info.Any())
			{
				foreach (var s in info)
				{
					LogInfoWithLineNumber(logger,
					                      s,
					                      file,
					                      member,
					                      line);
				}
			}
		}

		public static void LogInfoWithLineNumber(this ILog logger,
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