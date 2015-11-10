using System;
using System.Web.Http;

namespace Teleopti.Ccc.Web.Areas.PerformanceTool.Controllers
{
	[LocalHostAccess]
	public class DangerousApiController : ApiController
	{
		[Route("api/Dangerous/ThrowDivideByZeroException"), HttpGet]
		public int ThrowDivideByZeroException()
		{
			var zero = 0;
			var number = 1;
			return number / zero;
		}

		[Route("api/Dangerous/ThrowException"), HttpGet]
		public void ThrowException()
		{
			throw new Exception("Testing");
		}

	}
}