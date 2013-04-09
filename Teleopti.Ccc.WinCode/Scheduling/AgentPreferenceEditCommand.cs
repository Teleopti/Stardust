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
		private readonly TimeSpan? _minStart;
		private readonly TimeSpan? _maxStart;
		private readonly TimeSpan? _minEnd;
		private readonly TimeSpan? _maxEnd;
		private readonly TimeSpan? _minLength;
		private readonly TimeSpan? _maxLength;
		private readonly bool _endNextDay;
		private readonly IAgentPreferenceDayCreator _agentPreferenceDayCreator;

		public AgentPreferenceEditCommand(IScheduleDay scheduleDay, TimeSpan? minStart, TimeSpan? maxStart, TimeSpan? minEnd, TimeSpan? maxEnd, TimeSpan? minLength, TimeSpan? maxLength, bool endNextDay, IAgentPreferenceDayCreator agentPreferenceDayCreator)
		{
			_scheduleDay = scheduleDay;
			_minStart = minStart;
			_maxStart = maxStart;
			_minEnd = minEnd;
			_maxEnd = maxEnd;
			_minLength = minLength;
			_maxLength = maxLength;
			_endNextDay = endNextDay;
			_agentPreferenceDayCreator = agentPreferenceDayCreator;
		}

		public void Execute()
		{
			if (CanExecute())
			{
				var preferenceDay = _agentPreferenceDayCreator.Create(_scheduleDay, _minStart, _maxStart, _minEnd, _maxEnd, _minLength, _maxLength, _endNextDay);
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
					var result = _agentPreferenceDayCreator.CanCreate(_minStart, _maxStart, _minEnd, _maxEnd, _minLength, _maxLength, _endNextDay);
					return result.Result;
				}
			}

			return false;
		}
	}
}
