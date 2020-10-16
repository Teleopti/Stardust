#if NET472
using System.Web.Http.ExceptionHandling;
using Stardust.Manager.Extensions;

namespace Stardust.Manager
{
	public class GlobalExceptionLogger : ExceptionLogger
	{
		public override void Log(ExceptionLoggerContext context)
		{
			this.Log().ErrorWithLineNumber($"[Message] \n  {context.Exception.Message} \n\n [Stacktrace]\n  {context.Exception.StackTrace}");
		}
	}
}
#endif