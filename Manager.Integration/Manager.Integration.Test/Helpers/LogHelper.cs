using System;
using System.IO;
using System.Runtime.CompilerServices;
using log4net;
using log4net.Repository.Hierarchy;

namespace Manager.Integration.Test.Helpers
{
    public static class LogHelper
    {
        public static void LogErrorWithLineNumber(string info,
                                                  ILog logger,
                                                  System.Exception exception = null,
                                                  [CallerFilePath] string file = "",
                                                  [CallerMemberName] string member = "",
                                                  [CallerLineNumber] int line = 0)
        {
            ValidateArgument(logger);

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

        private static void ValidateArgument(ILog logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
        }

        public static void LogFatalWithLineNumber(string info,
                                                  ILog logger,
                                                  System.Exception exception = null,
                                                  [CallerFilePath] string file = "",
                                                  [CallerMemberName] string member = "",
                                                  [CallerLineNumber] int line = 0)
        {
            ValidateArgument(logger);

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

        public static void LogWarningWithLineNumber(string info,
                                                    ILog logger,
                                                    [CallerFilePath] string file = "",
                                                    [CallerMemberName] string member = "",
                                                    [CallerLineNumber] int line = 0)
        {
            ValidateArgument(logger);

            if (logger.IsWarnEnabled)
            {
                logger.Warn(string.Format("{0}_{1}({2}): {3}",
                                          Path.GetFileName(file),
                                          member,
                                          line,
                                          info));
            }
        }

        public static void LogDebugWithLineNumber(string info,
                                                  ILog logger,
                                                  [CallerFilePath] string file = "",
                                                  [CallerMemberName] string member = "",
                                                  [CallerLineNumber] int line = 0)
        {
            ValidateArgument(logger);

            if (logger.IsDebugEnabled)
            {
                logger.Debug(string.Format("{0}_{1}({2}): {3}",
                                           Path.GetFileName(file),
                                           member,
                                           line,
                                           info));
            }
        }

        public static void LogInfoWithLineNumber(string info,
                                                 ILog logger,
                                                 [CallerFilePath] string file = "",
                                                 [CallerMemberName] string member = "",
                                                 [CallerLineNumber] int line = 0)
        {
            ValidateArgument(logger);

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