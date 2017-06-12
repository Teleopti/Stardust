using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Staffing
{
    public class SendUpdateStaffingReadModelHandler : IHandleEvent<TenantMinuteTickEvent>, IRunOnHangfire
    {
	    private readonly IJobStartTimeRepository _jobStartTimeRepository;
        private readonly IBusinessUnitRepository _businessUnitRepository;
	    private readonly IRequestStrategySettingsReader _requestStrategySettingsReader;
	    private readonly IUpdatedBySystemUser _updatedBySystemUser;
	    private readonly IEventPublisher _publisher;
		private readonly IStaffingSettingsReader _staffingSettingsReader;

		public SendUpdateStaffingReadModelHandler(IBusinessUnitRepository businessUnitRepository, 
			IRequestStrategySettingsReader requestStrategySettingsReader, IJobStartTimeRepository jobStartTimeRepository, 
			IUpdatedBySystemUser updatedBySystemUser, IEventPublisher publisher, IStaffingSettingsReader staffingSettingsReader)
        {
            _businessUnitRepository = businessUnitRepository;
	        _requestStrategySettingsReader = requestStrategySettingsReader;
	        _jobStartTimeRepository = jobStartTimeRepository;
	        _updatedBySystemUser = updatedBySystemUser;
	        _publisher = publisher;
			_staffingSettingsReader = staffingSettingsReader;
		}

		[UnitOfWork]
		public virtual void Handle(TenantMinuteTickEvent @event)
        {
		    var updateResourceReadModelIntervalMinutes =
			    _requestStrategySettingsReader.GetIntSetting("UpdateResourceReadModelIntervalMinutes", 60);

			var businessUnits = _businessUnitRepository.LoadAll();
	        using (_updatedBySystemUser.Context())
	        {
				businessUnits.ForEach(businessUnit =>
				{
					var businessUnitId = businessUnit.Id.GetValueOrDefault();
					if (!_jobStartTimeRepository.CheckAndUpdate(updateResourceReadModelIntervalMinutes, businessUnitId)) return;

					_publisher.Publish(new UpdateStaffingLevelReadModelEvent
					{
						Days = _staffingSettingsReader.GetIntSetting("StaffingReadModelNumberOfDays", 14),
						LogOnBusinessUnitId = businessUnitId
					});

				});
			}
		}
    }
}
