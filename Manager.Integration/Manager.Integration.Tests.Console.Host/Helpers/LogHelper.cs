using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using log4net;
using log4net.Repository.Hierarchy;

namespace Manager.IntegrationTest.Console.Host.Helpers
{
    public static class LogHelper
    {
        public static void LogErrorWithLineNumber(ILog logger,
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

        public static void LogFatalWithLineNumber(ILog logger,
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

        public static void LogWarningWithLineNumber(ILog logger,
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

        public static void LogDebugWithLineNumber(ILog logger,
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

        public static void LogInfoWithLineNumber(ILog logger,
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

        public static void LogInfoWithLineNumber(ILog logger,
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