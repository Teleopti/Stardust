using System;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface  IAgentPreferenceDayCreator
	{
		IPreferenceDay Create(IScheduleDay scheduleDay, TimeSpan? minStart, TimeSpan? maxStart, TimeSpan? minEnd, TimeSpan? maxEnd, TimeSpan? minLength, TimeSpan? maxLength, bool endNextDay);
		IAgentPreferenceCanCreateResult CanCreate(TimeSpan? minStart, TimeSpan? maxStart, TimeSpan? minEnd, TimeSpan? maxEnd, TimeSpan? minLength, TimeSpan? maxLength, bool endNextDay);
	}

	public class AgentPreferenceDayCreator : IAgentPreferenceDayCreator
	{
		public IPreferenceDay Create(IScheduleDay scheduleDay, TimeSpan? minStart, TimeSpan? maxStart, TimeSpan? minEnd, TimeSpan? maxEnd, TimeSpan? minLength, TimeSpan? maxLength, bool endNextDay)
		{
			if(scheduleDay == null) throw new ArgumentNullException("scheduleDay");

			var result = CanCreate(minStart, maxStart, minEnd, maxEnd, minLength, maxLength, endNextDay);
			if (!result.Result) return null;

			var restriction = new PreferenceRestriction();
			restriction.StartTimeLimitation = new StartTimeLimitation(minStart, maxStart);
			restriction.EndTimeLimitation = new EndTimeLimitation(minEnd, maxEnd);
			restriction.WorkTimeLimitation = new WorkTimeLimitation(minLength, maxLength);
			var preferenceDay = new PreferenceDay(scheduleDay.Person, scheduleDay.DateOnlyAsPeriod.DateOnly, restriction);

			return preferenceDay;
		}

		public IAgentPreferenceCanCreateResult CanCreate(TimeSpan? minStart, TimeSpan? maxStart, TimeSpan? minEnd, TimeSpan? maxEnd, TimeSpan? minLength, TimeSpan? maxLength, bool endNextDay)
		{
			var result = new AgentPreferenceCanCreateResult();
			result.Result = true;

			if(endNextDay && maxEnd != null)
				maxEnd = maxEnd.Value.Add(TimeSpan.FromDays(1));

			result = validateStartTimes(minStart, maxStart, result);
			if (!result.Result) return result;

			result = validateEndTimes(minEnd, maxEnd, result);
			if (!result.Result) return result;

			result = validateLengths(minLength, maxLength, result);
			if (!result.Result) return result;

			result = validateStartTimesVersusEndTimes(minStart, maxStart, minEnd, maxEnd, result);
			if (!result.Result) return result;

			result = validateLengthsVersusTimes(minStart, maxStart, minEnd, maxEnd, minLength, maxLength, result);
			if (!result.Result) return result;

			return result;
		}

		private static AgentPreferenceCanCreateResult validateStartTimes(TimeSpan? minStart, TimeSpan? maxStart, AgentPreferenceCanCreateResult result)
		{
			if (minStart != null && maxStart != null)
			{
				if (minStart.Value > maxStart.Value)
				{
					result.Result = false;
					result.StartTimeMinError = true;
				}
			}

			return result;
		}

		private static AgentPreferenceCanCreateResult validateEndTimes(TimeSpan? minEnd, TimeSpan? maxEnd, AgentPreferenceCanCreateResult result)
		{
			if (minEnd != null && maxEnd != null)
			{
				if (minEnd.Value > maxEnd.Value)
				{
					result.Result = false;
					result.EndTimeMinError = true;
				}
			}

			return result;
		}

		private static AgentPreferenceCanCreateResult validateLengths(TimeSpan? minLength, TimeSpan? maxLength, AgentPreferenceCanCreateResult result)
		{
			if (minLength != null && maxLength != null)
			{
				if (minLength.Value > maxLength.Value)
				{
					result.Result = false;
					result.LengthMinError = true;
				}
			}

			return result;
		}

		private static AgentPreferenceCanCreateResult validateStartTimesVersusEndTimes(TimeSpan? minStart, TimeSpan? maxStart, TimeSpan? minEnd, TimeSpan? maxEnd, AgentPreferenceCanCreateResult result)
		{
			if (minStart != null)
			{
				if (minEnd != null && minStart.Value >= minEnd.Value)
				{
					result.Result = false;
					result.StartTimeMinError = true;
					return result;
				}

				if (maxEnd != null && minStart.Value >= maxEnd.Value)
				{
					result.Result = false;
					result.StartTimeMinError = true;
					return result;
				}
			}

			if (maxStart != null)
			{
				if (minEnd != null && maxStart.Value >= minEnd.Value)
				{
					result.Result = false;
					result.StartTimeMaxError = true;
					return result;
				}

				if (maxEnd != null && maxStart.Value >= maxEnd.Value)
				{
					result.Result = false;
					result.StartTimeMaxError = true;
					return result;
				}
			}

			return result;
		}

		private static AgentPreferenceCanCreateResult validateLengthsVersusTimes(TimeSpan? minStart, TimeSpan? maxStart, TimeSpan? minEnd, TimeSpan? maxEnd,  TimeSpan? minLength,  TimeSpan? maxLength, AgentPreferenceCanCreateResult result)
		{
			if (minLength != null)
			{
				if (minStart != null && maxEnd != null)
				{
					var maximumLength = maxEnd.Value.Subtract(minStart.Value);

					if (minLength > maximumLength)
					{
						result.Result = false;
						result.LengthMinError = true;
						return result;
					}
				}
			}

			if (maxLength != null)
			{
				if (maxStart != null && minEnd != null)
				{
					var minumumLength = minEnd.Value.Subtract(maxStart.Value);
					{
						if (maxLength < minumumLength)
						{
							result.Result = false;
							result.LengthMaxError = true;
							return result;
						}
					}
				}
			}

			return result;
		}
	}
}
