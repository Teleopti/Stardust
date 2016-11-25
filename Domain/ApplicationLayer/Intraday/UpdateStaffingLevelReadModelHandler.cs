using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Intraday
{
	[EnabledBy(Toggles.AbsenceRequests_SpeedupIntradayRequests_40754)]
	public class UpdateStaffingLevelReadModelHandler : IHandleEvent<UpdateStaffingLevelReadModelEvent>, IRunOnStardust
	{
		private readonly IUpdateStaffingLevelReadModel _updateStaffingLevelReadModel;
		private readonly IScheduleForecastSkillReadModelRepository _scheduleForecastSkillReadModelRepository;
		private readonly ICurrentUnitOfWorkFactory _currentFactory;
		private readonly INow _now;
		private readonly IRequestStrategySettingsReader _requestStrategySettingsReader;

		public UpdateStaffingLevelReadModelHandler(IUpdateStaffingLevelReadModel updateStaffingLevelReadModel, ICurrentUnitOfWorkFactory current, INow now, IScheduleForecastSkillReadModelRepository scheduleForecastSkillReadModelRepository, IRequestStrategySettingsReader requestStrategySettingsReader)
		{
			_updateStaffingLevelReadModel = updateStaffingLevelReadModel;
			_currentFactory = current;
			_now = now;
			_scheduleForecastSkillReadModelRepository = scheduleForecastSkillReadModelRepository;
			_requestStrategySettingsReader = requestStrategySettingsReader;
		}

		[AsSystem]
		[UnitOfWork]
		public virtual void Handle(UpdateStaffingLevelReadModelEvent @event)
		{
			if (@event.RequestedFromWeb)
				_scheduleForecastSkillReadModelRepository.UpdateInsertedDateTime(@event.LogOnBusinessUnitId);
			else
			{
				var lastCalculatedDates = _scheduleForecastSkillReadModelRepository.GetLastCalculatedTime();
				if (lastCalculatedDates.ContainsKey(@event.LogOnBusinessUnitId))
				{
					var updateResourceReadModelIntervalMinutes =
					   _requestStrategySettingsReader.GetIntSetting("UpdateResourceReadModelIntervalMinutes", 60);
					var lastCalculateDateTime = lastCalculatedDates[@event.LogOnBusinessUnitId];
					var now = _now.UtcDateTime();
					if (lastCalculateDateTime.AddMinutes(updateResourceReadModelIntervalMinutes) >= now)
					{
						return;
					}
					_scheduleForecastSkillReadModelRepository.UpdateInsertedDateTime(@event.LogOnBusinessUnitId);
				}
			}
			var period = new DateTimePeriod(@event.StartDateTime, @event.EndDateTime);
			_updateStaffingLevelReadModel.Update(period);
			var current = _currentFactory.Current().CurrentUnitOfWork();
			//an ugly solution for bug 39594
			if (current.IsDirty())
				current.Clear();
		}
	}
}
