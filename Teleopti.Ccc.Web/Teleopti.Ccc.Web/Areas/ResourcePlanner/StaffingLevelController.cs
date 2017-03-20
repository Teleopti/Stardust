using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class StaffingLevelController : ApiController
	{
		private readonly IEventPublisher _publisher;
		private readonly IJobStartTimeRepository _jobStartTimeRepository;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;

		public StaffingLevelController(IEventPublisher publisher, IJobStartTimeRepository jobStartTimeRepository, ICurrentBusinessUnit currentBusinessUnit)
		{
			_publisher = publisher;
			_jobStartTimeRepository = jobStartTimeRepository;
			_currentBusinessUnit = currentBusinessUnit;
		}

		[UnitOfWork, HttpGet, Route("TriggerResourceCalculate")]
		public virtual IHttpActionResult TriggerResourceCalculate()
		{
			var bu = _currentBusinessUnit.Current().Id.GetValueOrDefault();
			_jobStartTimeRepository.Update(bu);
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
