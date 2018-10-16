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

		[UnitOfWork, HttpGet, Route("api/Reports/getallstaffingaudit")]
		public virtual IHttpActionResult GetAllStaffingAudit(Guid personId, DateTime startDate, DateTime endDate)
		{
			return Ok(_auditAggregatorService.Load(personId,startDate,endDate));
		}

	}

}