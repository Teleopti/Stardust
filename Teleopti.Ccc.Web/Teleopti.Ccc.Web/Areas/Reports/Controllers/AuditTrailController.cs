using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Reports.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.GeneralAuditTrailWebReport)]
	public class AuditTrailController : ApiController
	{
		private readonly IAuditAggregatorService _auditAggregatorService;

		public AuditTrailController(IAuditAggregatorService auditAggregatorService)
		{
			_auditAggregatorService = auditAggregatorService;
		}

		[UnitOfWork, HttpGet, Route("api/reports/getauditlogs")]
		public virtual IHttpActionResult GetStaffingAudit(Guid personId, DateTime startDate, DateTime endDate, string searchword = "")
		{
			return Ok(new { AuditEntries = _auditAggregatorService.Load(personId,startDate,endDate, searchword) });
		}

	}

}