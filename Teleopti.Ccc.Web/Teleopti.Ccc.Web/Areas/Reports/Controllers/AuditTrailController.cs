using System;
using System.Globalization;
using System.Web.Http;
using NPOI.SS.Formula.Functions;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.Web.Areas.Staffing.Controllers
{
	public class AuditTrailController : ApiController
	{
		private readonly AuditService _auditService;

		public AuditTrailController(AuditService staffingAuditRepository)
		{
			_auditService = staffingAuditRepository;
		}

		[UnitOfWork, HttpGet, Route("api/Reports/getallstaffingaudit")]
		public virtual IHttpActionResult GetAllStaffingAudit()
		{
			return Ok(_auditService.LoadAll());
		}

	}

}