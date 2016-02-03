using System.Web.Http.ExceptionHandling;
using log4net;
using Stardust.Node.Helpers;

namespace Stardust.Node
{
    internal class GlobalExceptionLogger : ExceptionLogger
    {

        public override void Log(ExceptionLoggerContext context)
        {
            LogHelper.LogErrorWithLineNumber("[Message] \n" + context.Exception.Message + " \n\n [Stacktrace]\n " +
                         context.Exception.StackTrace);
        }
    }
}