using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class JavascriptLoggingController : ApiController
	{
		private readonly ILogManagerWrapper _logManagerWrapper;
		private const string logOutput = "{0} on url {1} at line {2}. The parent url is {3} and the user agent is {4}.";

		public JavascriptLoggingController(ILogManagerWrapper logManagerWrapper)
		{
			_logManagerWrapper = logManagerWrapper;
		}

		[HttpPost, Route("api/Logging/LogError")]
		public void LogError(JavascriptLog javascriptLog)
		{
			_logManagerWrapper.GetLogger("Teleopti.Javascript")
				.Error(string.Format(logOutput, javascriptLog.Message, javascriptLog.Url, javascriptLog.LineNumber, javascriptLog.ParentUrl, javascriptLog.UserAgent));
		}
	}
}