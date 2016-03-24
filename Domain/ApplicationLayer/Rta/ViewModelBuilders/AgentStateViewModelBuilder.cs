using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModelBuilders
{
	public interface IAgentStateViewModelBuilder
	{
		IEnumerable<AgentStatusViewModel> Build(IEnumerable<AgentStateReadModel> agentStates);
	}

	public class AgentStateViewModelBuilder : IAgentStateViewModelBuilder
	{
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		private readonly IUserCulture _culture;
		private readonly IAppliedAlarm _appliedAlarm;

		public AgentStateViewModelBuilder(INow now, IUserTimeZone timeZone, IUserCulture culture, IAppliedAlarm appliedAlarm)
		{
			_now = now;
			_timeZone = timeZone;
			_culture = culture;
			_appliedAlarm = appliedAlarm;
		}

		public IEnumerable<AgentStatusViewModel> Build(IEnumerable<AgentStateReadModel> states)
		{
			return states.Select(x =>
			{
				var timeInAlarm = calculateTimeInAlarm(x);
				return new AgentStatusViewModel
				{
					PersonId = x.PersonId,
					State = x.StateName,
					StateStartTime = x.StateStartTime,
					Activity = x.Scheduled,
					NextActivity = x.ScheduledNext,
					NextActivityStartTime = formatTime(x.NextStart),
					Alarm = x.RuleName,
					AlarmStart = x.AlarmStartTime,
					Color = _appliedAlarm.ColorTransition(x, timeInAlarm),
					TimeInState = x.StateStartTime.HasValue ? (int) (_now.UtcDateTime() - x.StateStartTime.Value).TotalSeconds : 0,
					TimeInAlarm = timeInAlarm
				};
			});
		}

		private int? calculateTimeInAlarm(AgentStateReadModel x)
		{
			if (x.AlarmStartTime.HasValue)
			{
				return _now.UtcDateTime() >= x.AlarmStartTime.Value ? (int?)(_now.UtcDateTime() - x.AlarmStartTime.Value).TotalSeconds : null;
			}
			return null;
		}

		private string formatTime(DateTime? timestamp)
		{
			if (!timestamp.HasValue) return null;
			var userTime = TimeZoneInfo.ConvertTimeFromUtc(timestamp.Value, _timeZone.TimeZone());
			var today = TimeZoneInfo.ConvertTimeFromUtc(_now.UtcDateTime(), _timeZone.TimeZone()).Date;

			return userTime < today.AddDays(1)
				? userTime.ToString(_culture.GetCulture().DateTimeFormat.ShortTimePattern)
				: userTime.ToString(_culture.GetCulture().DateTimeFormat.ShortDatePattern) + " " +
				  userTime.ToString(_culture.GetCulture().DateTimeFormat.ShortTimePattern)
				;
		}
	}
}