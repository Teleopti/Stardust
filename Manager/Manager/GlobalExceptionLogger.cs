using System.Web.Http.ExceptionHandling;
using log4net;
using Stardust.Manager.Helpers;

namespace Stardust.Manager
{
	public class GlobalExceptionLogger : ExceptionLogger
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (GlobalExceptionLogger));

		public override void Log(ExceptionLoggerContext context)
		{
			LogHelper.LogErrorWithLineNumber(Logger,
			                                 "[Message] \n" + context.Exception.Message + " \n\n [Stacktrace]\n " +
			                                 context.Exception.StackTrace);
		}
	}
}