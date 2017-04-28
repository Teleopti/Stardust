using System;
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
	    private readonly IUpdateStaffingLevelReadModelSender _updateStaffingLevelReadModelSender;

		public SendUpdateStaffingReadModelHandler(IBusinessUnitRepository businessUnitRepository, 
			IRequestStrategySettingsReader requestStrategySettingsReader, IJobStartTimeRepository jobStartTimeRepository, IUpdatedBySystemUser updatedBySystemUser, 
			IUpdateStaffingLevelReadModelSender updateStaffingLevelReadModelSender)
        {
            _businessUnitRepository = businessUnitRepository;
	        _requestStrategySettingsReader = requestStrategySettingsReader;
	        _jobStartTimeRepository = jobStartTimeRepository;
	        _updatedBySystemUser = updatedBySystemUser;
	        _updateStaffingLevelReadModelSender = updateStaffingLevelReadModelSender;
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
					_updateStaffingLevelReadModelSender.Send(businessUnitId);
				});
			}
		}
    }

	public class UpdateStaffingLevelReadModelSender1Day : IUpdateStaffingLevelReadModelSender
	{
		private readonly IEventPublisher _publisher;

		public UpdateStaffingLevelReadModelSender1Day(IEventPublisher publisher)
		{
			_publisher = publisher;
		}

		public void Send(Guid businessUnitId)
		{
			_publisher.Publish(new UpdateStaffingLevelReadModelEvent
			{
				Days = 1,
				LogOnBusinessUnitId = businessUnitId
			});
		}
	}

	public class UpdateStaffingLevelReadModelSender14Days : IUpdateStaffingLevelReadModelSender
	{
		private readonly IEventPublisher _publisher;

		public UpdateStaffingLevelReadModelSender14Days(IEventPublisher publisher)
		{
			_publisher = publisher;
		}

		public void Send(Guid businessUnitId)
		{
			_publisher.Publish(new UpdateStaffingLevelReadModelEvent
			{
				Days = 14,
				LogOnBusinessUnitId = businessUnitId
			});
		}
	}

	public interface IUpdateStaffingLevelReadModelSender
	{
		void Send(Guid businessUnitId);
	}

	public class IntradayResourceCalculationForAbsenceHandler : IHandleEvent<TenantMinuteTickEvent>, IRunOnHangfire
	{
		public virtual void Handle(TenantMinuteTickEvent @event)
		{
			// If there are any IntradayResourceCalculationForAbsenceHandler leftovers in the queue after update
		}
	}
}
