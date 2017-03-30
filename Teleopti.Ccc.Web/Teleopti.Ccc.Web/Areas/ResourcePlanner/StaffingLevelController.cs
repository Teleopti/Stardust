using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class StaffingLevelController : ApiController
	{
		private readonly IEventPublisher _publisher;

		public StaffingLevelController(IEventPublisher publisher)
		{
			_publisher = publisher;
		}

		[UnitOfWork, HttpGet, Route("TriggerResourceCalculate")]
		public virtual IHttpActionResult TriggerResourceCalculate()
		{
			_publisher.Publish(new UpdateStaffingLevelReadModelEvent
			{
				Days = 1
			});

			return Ok();
		}

		//[UnitOfWork, HttpGet, Route("GetLastCaluclatedDateTime")]
		//public virtual IHttpActionResult GetLastCaluclatedDateTime()
		//{
		//	var buid = ((TeleoptiIdentity) _currentTeleoptiPrincipal.Current().Identity).BusinessUnit.Id.GetValueOrDefault();
		//	return Json(getLastCaluclatedDateTime(buid));
		//}

		//private DateTime getLastCaluclatedDateTime(Guid buId)
		//{
			//var lastCalculated = _jobStartTimeRepository.LoadAll();
			//return lastCalculated.ContainsKey(buId) ? lastCalculated[buId] : DateTime.MinValue;
		//}
	}
}
