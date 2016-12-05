using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class JavascriptLoggingController : ApiController
	{
		private readonly ILogManager _logManager;
		private const string logOutput = "{0} on url {1} at line {2}. The parent url is {3} and the user agent is {4}.";

		public JavascriptLoggingController(ILogManager logManager)
		{
			_logManager = logManager;
		}

		[HttpPost, Route("api/Logging/LogError")]
		public void LogError(JavascriptLog javascriptLog)
		{
			_logManager.GetLogger("Teleopti.Javascript")
				.Warn(string.Format(logOutput, javascriptLog.Message, javascriptLog.Url, javascriptLog.LineNumber, javascriptLog.ParentUrl, javascriptLog.UserAgent));
		}
	}
}