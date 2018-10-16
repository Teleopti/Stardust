using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;

namespace Teleopti.Ccc.Web.Areas.Reports.Controllers
{
	public class AuditTrailController : ApiController
	{
		private readonly AuditAggregatorService _auditAggregatorService;

		public AuditTrailController(AuditAggregatorService auditAggregatorService)
		{
			_auditAggregatorService = auditAggregatorService;
		}

		[UnitOfWork, HttpGet, Route("api/reports/getauditlogs")]
		public virtual IHttpActionResult GetStaffingAudit(Guid personId, DateTime startDate, DateTime endDate)
		{
			return Ok(_auditAggregatorService.Load(personId,startDate,endDate));
		}

	}

}