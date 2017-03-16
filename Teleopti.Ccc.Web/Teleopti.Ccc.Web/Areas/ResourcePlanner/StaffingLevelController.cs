using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class StaffingLevelController : ApiController
	{
		private readonly IEventPublisher _publisher;
		private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;
		private readonly IJobStartTimeRepository _jobStartTimeRepository;

		public StaffingLevelController(IEventPublisher publisher,
			ICurrentTeleoptiPrincipal currentTeleoptiPrincipal, IJobStartTimeRepository jobStartTimeRepository)
		{
			_publisher = publisher;
			_currentTeleoptiPrincipal = currentTeleoptiPrincipal;
			_jobStartTimeRepository = jobStartTimeRepository;
		}

		[UnitOfWork, HttpGet, Route("TriggerResourceCalculate")]
		public virtual IHttpActionResult TriggerResourceCalculate()
		{
			_publisher.Publish(new UpdateStaffingLevelReadModelEvent
			{
				Days = 1,
				RequestedFromWeb = true
			});

			return Ok();
		}

		[UnitOfWork, HttpGet, Route("GetLastCaluclatedDateTime")]
		public virtual IHttpActionResult GetLastCaluclatedDateTime()
		{
			var buid = ((TeleoptiIdentity) _currentTeleoptiPrincipal.Current().Identity).BusinessUnit.Id.GetValueOrDefault();
			return Json(getLastCaluclatedDateTime(buid));
		}

		private DateTime getLastCaluclatedDateTime(Guid buId)
		{
			var lastCalculated = _jobStartTimeRepository.LoadAll();
			return lastCalculated.ContainsKey(buId) ? lastCalculated[buId] : DateTime.MinValue;
		}
	}
}
