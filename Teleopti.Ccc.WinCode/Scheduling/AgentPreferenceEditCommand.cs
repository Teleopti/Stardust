using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IAgentPreferenceEditCommand : IExecutableCommand, ICanExecute
	{
		//	
	}
	public class AgentPreferenceEditCommand : IAgentPreferenceEditCommand
	{
		private readonly IScheduleDay _scheduleDay;
		private readonly IShiftCategory _shiftCategory;
		private readonly IAbsence _absence;
		private readonly IDayOffTemplate _dayOffTemplate;
		private readonly IActivity _activity;
		private readonly TimeSpan? _minStart;
		private readonly TimeSpan? _maxStart;
		private readonly TimeSpan? _minEnd;
		private readonly TimeSpan? _maxEnd;
		private readonly TimeSpan? _minLength;
		private readonly TimeSpan? _maxLength;
		private readonly IAgentPreferenceDayCreator _agentPreferenceDayCreator;
		private readonly TimeSpan? _minStartActivity;
		private readonly TimeSpan? _maxStartActivity;
		private readonly TimeSpan? _minEndActivity;
		private readonly TimeSpan? _maxEndActivity;
		private readonly TimeSpan? _minLengthActivity;
		private readonly TimeSpan? _maxLengthActivity;

		public AgentPreferenceEditCommand(IScheduleDay scheduleDay, IShiftCategory shiftCategory, IAbsence absence, IDayOffTemplate dayOffTemplate, IActivity activity, TimeSpan? minStart, TimeSpan? maxStart, TimeSpan? minEnd, TimeSpan? maxEnd, TimeSpan? minLength, TimeSpan? maxLength, 
												  TimeSpan? minStartActivity, TimeSpan? maxStartActivity,
												  TimeSpan? minEndActivity, TimeSpan? maxEndActivity,
												  TimeSpan? minLengthActivity, TimeSpan? maxLengthActivity, IAgentPreferenceDayCreator agentPreferenceDayCreator)
		{
			_scheduleDay = scheduleDay;
			_shiftCategory = shiftCategory;
			_absence = absence;
			_dayOffTemplate = dayOffTemplate;
			_activity = activity;
			_minStart = minStart;
			_maxStart = maxStart;
			_minEnd = minEnd;
			_maxEnd = maxEnd;
			_minLength = minLength;
			_maxLength = maxLength;
			_agentPreferenceDayCreator = agentPreferenceDayCreator;
			_minStartActivity = minStartActivity;
			_maxStartActivity = maxStartActivity;
			_minEndActivity = minEndActivity;
			_maxEndActivity = maxEndActivity;
			_minLengthActivity = minLengthActivity;
			_maxLengthActivity = maxLengthActivity;
		}

		public void Execute()
		{
			if (CanExecute())
			{
				var preferenceDay = _agentPreferenceDayCreator.Create(_scheduleDay, _shiftCategory, _absence, _dayOffTemplate, _activity, _minStart, _maxStart, _minEnd, _maxEnd, _minLength, _maxLength, _minStartActivity, _maxStartActivity, _minEndActivity, _maxEndActivity, _minLengthActivity, _maxLengthActivity);
				if (preferenceDay != null)
				{
					_scheduleDay.DeletePreferenceRestriction();
					_scheduleDay.Add(preferenceDay);
				}
			}
		}

		public bool CanExecute()
		{
			foreach (var persistableScheduleData in _scheduleDay.PersistableScheduleDataCollection())
			{
				if (persistableScheduleData is IPreferenceDay)
				{
					var result = _agentPreferenceDayCreator.CanCreate(_shiftCategory, _absence, _dayOffTemplate, _activity, _minStart,
					                                                  _maxStart, _minEnd, _maxEnd, _minLength, _maxLength,
					                                                  _minStartActivity, _maxStartActivity, _minEndActivity,
					                                                  _maxEndActivity, _minLengthActivity, _maxLengthActivity);
					return result.Result;
				}
			}

			return false;
		}
	}
}
