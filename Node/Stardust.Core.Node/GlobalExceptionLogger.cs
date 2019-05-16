//using System.Web.Http.ExceptionHandling;

using Microsoft.Extensions.Logging;
//using log4net;

namespace Stardust.Core.Node
{
	internal class GlobalExceptionLogger //: ExceptionLogger
	{
		private static readonly ILogger Logger = new LoggerFactory().CreateLogger(typeof (GlobalExceptionLogger));

//		public override void Log(ExceptionLoggerContext context)
//		{
//			Logger.ErrorWithLineNumber("[Message] \n" + context.Exception.Message + " \n\n [Stacktrace]\n " +
//			                                 context.Exception.StackTrace);
//		}
	}
}