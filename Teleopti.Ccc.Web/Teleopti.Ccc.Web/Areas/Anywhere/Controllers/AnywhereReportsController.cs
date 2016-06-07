using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class AnywhereReportsController : ApiController
	{
		private readonly IReportItemsProvider _reportItemsProvider;
		public AnywhereReportsController(IReportItemsProvider reportItemsProvider)
		{
			_reportItemsProvider = reportItemsProvider;
		}

		[UnitOfWork, HttpGet, Route("Anywhere/Reports")]
		public virtual IHttpActionResult GetReports()
		{
			return Ok(_reportItemsProvider.GetReportItems());
		}
	}
}