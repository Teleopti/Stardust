using System.Web.Http;
using log4net;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class JavascriptLoggingController : ApiController
	{
		private readonly ILog _logger;

		public JavascriptLoggingController(ILog logger)
		{
			_logger = logger;
		}

		[HttpPost, Route("api/Logging/LogError")]
		public virtual void LogError(string content)
		{
			_logger.Error(content);
		}
	}
}