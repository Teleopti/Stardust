using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	[EnabledBy(Toggles.Wfm_Requests_ProcessWaitlistBefore24hRequests_45767)]
	public class WaitlistProcessPublisher : IHandleEvent<TenantDayTickEvent>, IRunOnHangfire
	{
		private readonly IEventPublisher _publisher;
		private readonly IBusinessUnitRepository _businessUnitRepository;
		private readonly IUpdatedBySystemUser _updatedBySystemUser;

		public WaitlistProcessPublisher(IEventPublisher publisher, IBusinessUnitRepository businessUnitRepository, IUpdatedBySystemUser updatedBySystemUser)
		{
			_publisher = publisher;
			_businessUnitRepository = businessUnitRepository;
			_updatedBySystemUser = updatedBySystemUser;
		}

		[UnitOfWork]
		public virtual void Handle(TenantDayTickEvent tenantDayTickEvent)
		{
			var businessUnits = _businessUnitRepository.LoadAll();
			using (_updatedBySystemUser.Context())
			{
				businessUnits.ForEach(businessUnit =>
				{
					var businessUnitId = businessUnit.Id.GetValueOrDefault();
					
					_publisher.Publish(new ProcessWaitlistedRequestsEvent
					{
						LogOnBusinessUnitId = businessUnitId
					});

				});
			}
		}
	}
}
