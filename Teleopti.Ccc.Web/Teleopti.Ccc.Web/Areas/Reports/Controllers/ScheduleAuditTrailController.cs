using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Reports;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Reports.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.ScheduleAuditTrailReport)]
	public class ScheduleAuditTrailController : ApiController
	{
		private readonly PersonsWhoChangedSchedulesViewModelProvider _personsWhoChangedSchedulesViewModelProvider;
		private readonly ScheduleAuditTrailReportViewModelProvider _scheduleAuditTrailReportViewModelProvider;

		public ScheduleAuditTrailController(PersonsWhoChangedSchedulesViewModelProvider personsWhoChangedSchedulesViewModelProvider,
			ScheduleAuditTrailReportViewModelProvider scheduleAuditTrailReportViewModelProvider)
		{
			_personsWhoChangedSchedulesViewModelProvider = personsWhoChangedSchedulesViewModelProvider;
			_scheduleAuditTrailReportViewModelProvider = scheduleAuditTrailReportViewModelProvider;
		}

		[UnitOfWork, HttpGet, Route("api/Reports/PersonsWhoChangedSchedules")]
		public virtual IHttpActionResult PersonsWhoChangedSchedules()
		{
			return Ok(_personsWhoChangedSchedulesViewModelProvider.Provide());
		}

		[UnitOfWork, HttpPost, Route("api/Reports/ScheduleAuditTrailReport")]
		public virtual IHttpActionResult ScheduleAuditTrailReport([FromBody] AuditTrailSearchParams value)
		{
			return Ok(_scheduleAuditTrailReportViewModelProvider.Provide(value));
		}
	}
}