using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class ReportsController : ApiController
	{
		private readonly IReportItemsProvider _reportItemsProvider;
		public ReportsController(IReportItemsProvider reportItemsProvider)
		{
			_reportItemsProvider = reportItemsProvider;
		}

		[UnitOfWork, HttpGet, Route("Anywhere/Reports/GetReports")]
		public virtual IHttpActionResult GetReports()
		{
			return Ok(_reportItemsProvider.GetReportItems());
		}
	}
}