using System.IO;
using System.Runtime.CompilerServices;
using log4net;

namespace Stardust.Node.Helpers
{
    public static class LogHelper
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(typeof(LogHelper));

        public static void LogErrorWithLineNumber(string info,
                                                  System.Exception exception = null,
                                                  [CallerFilePath] string file = "",
                                                  [CallerMemberName] string member = "",
                                                  [CallerLineNumber] int line = 0)
        {
            if (Logger.IsErrorEnabled)
            {
                Logger.Error(string.Format("{0}_{1}({2}): {3}",
                                           Path.GetFileName(file),
                                           member,
                                           line,
                                           info),
                             exception);
            }
        }

        public static void LogFatalWithLineNumber(string info,
                                                  System.Exception exception = null,
                                                  [CallerFilePath] string file = "",
                                                  [CallerMemberName] string member = "",
                                                  [CallerLineNumber] int line = 0)
        {
            if (Logger.IsFatalEnabled)
            {
                Logger.Fatal(string.Format("{0}_{1}({2}): {3}",
                                           Path.GetFileName(file),
                                           member,
                                           line,
                                           info),
                             exception);
            }
        }

        public static void LogWarningWithLineNumber(string info,
                                                    [CallerFilePath] string file = "",
                                                    [CallerMemberName] string member = "",
                                                    [CallerLineNumber] int line = 0)
        {
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn(string.Format("{0}_{1}({2}): {3}",
                                          Path.GetFileName(file),
                                          member,
                                          line,
                                          info));
            }
        }

        public static void LogDebugWithLineNumber(string info,
                                                  [CallerFilePath] string file = "",
                                                  [CallerMemberName] string member = "",
                                                  [CallerLineNumber] int line = 0)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug(string.Format("{0}_{1}({2}): {3}",
                                           Path.GetFileName(file),
                                           member,
                                           line,
                                           info));
            }
        }

        public static void LogInfoWithLineNumber(string info,
                                                 [CallerFilePath] string file = "",
                                                 [CallerMemberName] string member = "",
                                                 [CallerLineNumber] int line = 0)
        {
            if (Logger.IsInfoEnabled)
            {
                Logger.Info(string.Format("{0}_{1}({2}): {3}",
                                          Path.GetFileName(file),
                                          member,
                                          line,
                                          info));
            }
        }
    }
}