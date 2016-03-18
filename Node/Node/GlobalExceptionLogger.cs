using System.Web.Http.ExceptionHandling;
using log4net;
using Stardust.Node.Extensions;
using Stardust.Node.Helpers;
using Stardust.Node.Log4Net;
using Stardust.Node.Log4Net.Extensions;

namespace Stardust.Node
{
	internal class GlobalExceptionLogger : ExceptionLogger
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (GlobalExceptionLogger));

		public GlobalExceptionLogger()
		{
			
		}

		public override void Log(ExceptionLoggerContext context)
		{
			Logger.ErrorWithLineNumber("[Message] \n" + context.Exception.Message + " \n\n [Stacktrace]\n " +
			                                 context.Exception.StackTrace);
		}
	}
}