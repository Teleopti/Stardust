using System.Web.Http.ExceptionHandling;
using log4net;

namespace Stardust.Node
{
    internal class GlobalExceptionLogger : ExceptionLogger
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (GlobalExceptionLogger));

        public override void Log(ExceptionLoggerContext context)
        {
            Logger.Error("[Message] \n" + context.Exception.Message + " \n\n [Stacktrace]\n " +
                         context.Exception.StackTrace);
        }
    }
}