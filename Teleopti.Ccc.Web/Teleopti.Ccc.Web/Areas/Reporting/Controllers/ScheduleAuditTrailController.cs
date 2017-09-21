﻿using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Reports;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Reporting.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.ScheduleAuditTrailReport)]
	public class ScheduleAuditTrailController : ApiController
	{
		private readonly ScheduleChangedByUserViewModelProvider _scheduleChangedByUserViewModelProvider;

		public ScheduleAuditTrailController(ScheduleChangedByUserViewModelProvider scheduleChangedByUserViewModelProvider)
		{
			_scheduleChangedByUserViewModelProvider = scheduleChangedByUserViewModelProvider;
		}

		[UnitOfWork, HttpGet, Route("api/reporting/scheduleChangedByPersons")]
		public virtual IHttpActionResult ScheduleChangedByPersonsInAuditTrail()
		{
			return Ok(_scheduleChangedByUserViewModelProvider.Provide());
		}
	}
}