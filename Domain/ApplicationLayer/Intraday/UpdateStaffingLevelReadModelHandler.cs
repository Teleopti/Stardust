﻿using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Intraday
{
	public class UpdateStaffingLevelReadModelHandler : IHandleEvent<UpdateStaffingLevelReadModelEvent>, IRunOnStardust
	{
		private readonly IUpdateStaffingLevelReadModel _updateStaffingLevelReadModel;
		private readonly ICurrentUnitOfWorkFactory _currentFactory;
		private readonly INow _now;
		private readonly IRequestStrategySettingsReader _requestStrategySettingsReader;
		private readonly IStardustJobFeedback _feedback;
		private readonly IJobStartTimeRepository _jobStartTimeRepository;

		public UpdateStaffingLevelReadModelHandler(IUpdateStaffingLevelReadModel updateStaffingLevelReadModel, ICurrentUnitOfWorkFactory current, INow now, IRequestStrategySettingsReader requestStrategySettingsReader, IStardustJobFeedback feedback, IJobStartTimeRepository jobStartTimeRepository)
		{
			_updateStaffingLevelReadModel = updateStaffingLevelReadModel;
			_currentFactory = current;
			_now = now;
			_requestStrategySettingsReader = requestStrategySettingsReader;
			_feedback = feedback;
			_jobStartTimeRepository = jobStartTimeRepository;
		}

		[AsSystem]
		[UnitOfWork]
		public virtual void Handle(UpdateStaffingLevelReadModelEvent @event)
		{
			if (@event.RequestedFromWeb)
				_jobStartTimeRepository.Persist(@event.LogOnBusinessUnitId, _now.UtcDateTime());
			else
			{
				var lastCalculatedDates = _jobStartTimeRepository.LoadAll();
				if (lastCalculatedDates.ContainsKey(@event.LogOnBusinessUnitId))
				{
					var updateResourceReadModelIntervalMinutes =
					   _requestStrategySettingsReader.GetIntSetting("UpdateResourceReadModelIntervalMinutes", 60);
					var lastCalculateDateTime = lastCalculatedDates[@event.LogOnBusinessUnitId];
					var now = _now.UtcDateTime();
					if (lastCalculateDateTime.AddMinutes(updateResourceReadModelIntervalMinutes) >= now)
					{
						_feedback.SendProgress($"The job was recently executed at {lastCalculateDateTime}");
						return;
					}					
				}
				_jobStartTimeRepository.Persist(@event.LogOnBusinessUnitId, _now.UtcDateTime());
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
