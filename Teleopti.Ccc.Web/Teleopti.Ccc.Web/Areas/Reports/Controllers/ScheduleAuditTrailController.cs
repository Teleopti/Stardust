using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Reports;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Reports.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.ScheduleAuditTrailWebReport)]
	public class ScheduleAuditTrailController : ApiController
	{
		private readonly PersonsWhoChangedSchedulesViewModelProvider _personsWhoChangedSchedulesViewModelProvider;
		private readonly ScheduleAuditTrailReportViewModelProvider _scheduleAuditTrailReportViewModelProvider;
		private readonly OrganizationSelectionProvider _organizationSelectionProvider;

		public ScheduleAuditTrailController(PersonsWhoChangedSchedulesViewModelProvider personsWhoChangedSchedulesViewModelProvider,
			ScheduleAuditTrailReportViewModelProvider scheduleAuditTrailReportViewModelProvider, OrganizationSelectionProvider organizationSelectionProvider)
		{
			_personsWhoChangedSchedulesViewModelProvider = personsWhoChangedSchedulesViewModelProvider;
			_scheduleAuditTrailReportViewModelProvider = scheduleAuditTrailReportViewModelProvider;
			_organizationSelectionProvider = organizationSelectionProvider;
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


		[UnitOfWork, HttpGet, Route("api/Reports/OrganizationSelectionAuditTrail")]
		public virtual object OrganizationSelectionAuditTrail()
		{
			return _organizationSelectionProvider.Provide(false);
		}
	}
}