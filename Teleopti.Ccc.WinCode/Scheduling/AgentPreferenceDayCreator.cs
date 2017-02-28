using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface  IAgentPreferenceDayCreator
	{
		IPreferenceDay Create(IScheduleDay scheduleDay, IAgentPreferenceData data);

		IAgentPreferenceCanCreateResult CanCreate(IAgentPreferenceData data	);
	}

	public class AgentPreferenceDayCreator : IAgentPreferenceDayCreator
	{
		public IPreferenceDay Create(IScheduleDay scheduleDay, IAgentPreferenceData data)

		{	
			if(scheduleDay == null) throw new ArgumentNullException("scheduleDay");
			if(data == null) throw new ArgumentNullException("data");

			var result = CanCreate(data);
			if (!result.Result) return null;
	
			var restriction = new PreferenceRestriction
				{
					ShiftCategory = data.ShiftCategory,
					Absence = data.Absence,
					DayOffTemplate = data.DayOffTemplate,
					MustHave = data.MustHave
				};

			if (data.Activity != null)
			{
				var activityRestriction = new ActivityRestriction(data.Activity)
					{
						StartTimeLimitation = new StartTimeLimitation(data.MinStartActivity, data.MaxStartActivity),
						EndTimeLimitation = new EndTimeLimitation(data.MinEndActivity, data.MaxEndActivity),
						WorkTimeLimitation = new WorkTimeLimitation(data.MinLengthActivity, data.MaxLengthActivity)
					};

				restriction.AddActivityRestriction(activityRestriction);
			}

			if (data.Absence == null && data.DayOffTemplate == null)
			{
				restriction.StartTimeLimitation = new StartTimeLimitation(data.MinStart, data.MaxStart);
				restriction.EndTimeLimitation = new EndTimeLimitation(data.MinEnd, data.MaxEnd);
				restriction.WorkTimeLimitation = new WorkTimeLimitation(data.MinLength, data.MaxLength);
			}

			var preferenceDay = new PreferenceDay(scheduleDay.Person, scheduleDay.DateOnlyAsPeriod.DateOnly, restriction);

			return preferenceDay;
		}

		public IAgentPreferenceCanCreateResult CanCreate(IAgentPreferenceData data)
		{
			if (data == null) throw new ArgumentNullException("data");

			var result = new AgentPreferenceCanCreateResult {Result = true};

			if (validateEmpty(data))
			{
				result.Result = false;
				result.EmptyError = true;
				return result;
			}

			if (!validateStartTimes(data.MinStart, data.MaxStart))
			{
				result.Result = false;
				result.StartTimeMinError = true;
			}

			if (!validateStartTimes(data.MinStartActivity, data.MaxStartActivity))
			{
				result.Result = false;
				result.StartTimeMinErrorActivity = true;
			}

			if (!validateEndTimes(data.MinEnd, data.MaxEnd))
			{
				result.Result = false;
				result.EndTimeMinError = true;
			}

			if (!validateEndTimes(data.MinEndActivity, data.MaxEndActivity))
			{
				result.Result = false;
				result.EndTimeMinErrorActivity = true;
			}

			if (!validateLengths(data.MinLength, data.MaxLength))
			{
				result.Result = false;
				result.LengthMinError = true;
			}

			if (!validateLengths(data.MinLengthActivity, data.MaxLengthActivity))
			{
				result.Result = false;
				result.LengthMinErrorActivity = true;
			}

			if (!validateStartTimesMinVersusEndTimes(data.MinStart, data.MinEnd, data.MaxEnd))
			{
				result.Result = false;
				result.StartTimeMinError = true;
			}

			if (!validateStartTimesMinVersusEndTimes(data.MinStartActivity, data.MinEndActivity, data.MaxEndActivity))
			{
				result.Result = false;
				result.StartTimeMinErrorActivity = true;
			}

			if (!validateStartTimesMaxVersusEndTimes(data.MaxStart, data.MinEnd, data.MaxEnd))
			{
				result.Result = false;
				result.StartTimeMaxError = true;
			}

			if (!validateStartTimesMaxVersusEndTimes(data.MaxStartActivity, data.MinEndActivity, data.MaxEndActivity))
			{
				result.Result = false;
				result.StartTimeMaxErrorActivity = true;
			}

			if (!validateLengthMinVersusTimes(data.MinStart, data.MaxEnd, data.MinLength))
			{
				result.Result = false;
				result.LengthMinError = true;
			}

			if (!validateLengthMinVersusTimes(data.MinStartActivity, data.MaxEndActivity, data.MinLengthActivity))
			{
				result.Result = false;
				result.LengthMinErrorActivity = true;
			}

			if (!validateLengthMaxVersusTimes(data.MaxStart, data.MinEnd, data.MaxLength))
			{
				result.Result = false;
				result.LengthMaxError = true;
			}

			if (!validateLengthMaxVersusTimes(data.MaxStartActivity, data.MinEndActivity, data.MaxLengthActivity))
			{
				result.Result = false;
				result.LengthMaxErrorActivity = true;
			}

			result = validTypes(data.ShiftCategory, data.Absence, data.DayOffTemplate, data.Activity, result);
			

			return result;
		}

		private static bool validateEmpty(IAgentPreferenceData data)
		{
			if (data.ShiftCategory == null && data.Absence == null && data.DayOffTemplate == null && data.Activity == null && data.MinStart == null
				&& data.MaxStart == null && data.MinEnd == null && data.MaxEnd == null && data.MinLength == null && data.MaxLength == null &&
				data.MinStartActivity == null
				&& data.MaxStartActivity == null && data.MinEndActivity == null && data.MaxEndActivity == null && data.MinLengthActivity == null &&
				data.MaxLengthActivity == null) return true;

				return false;
		
		}

		private static bool validateStartTimes(TimeSpan? minStart, TimeSpan? maxStart)
		{
			if (minStart != null && maxStart != null)
			{
				if (minStart.Value > maxStart.Value)
				{
					return false;
				}
			}

			return true;
		}


		private static bool validateEndTimes(TimeSpan? minEnd, TimeSpan? maxEnd)
		{
			if (minEnd != null && maxEnd != null)
			{
				if (minEnd.Value > maxEnd.Value)
				{
					return false;
				}
			}

			return true;
		}

		
		private static bool validateLengths(TimeSpan? minLength, TimeSpan? maxLength)
		{
			if (minLength != null && maxLength != null)
			{
				if (minLength.Value > maxLength.Value)
				{
					return false;
				}
			}

			return true;
		}

		private static bool validateStartTimesMinVersusEndTimes(TimeSpan? minStart, TimeSpan? minEnd, TimeSpan? maxEnd)
		{
			if (minStart != null)
			{
				if (minEnd != null && minStart.Value >= minEnd.Value)
				{
					return false;
				}

				if (maxEnd != null && minStart.Value >= maxEnd.Value)
				{
					return false;
				}
			}


			return true;
		}

		private static bool validateStartTimesMaxVersusEndTimes(TimeSpan? maxStart, TimeSpan? minEnd, TimeSpan? maxEnd)
		{
			if (maxStart != null)
			{
				if (minEnd != null && maxStart.Value >= minEnd.Value)
				{
					return false;
				}

				if (maxEnd != null && maxStart.Value >= maxEnd.Value)
				{
					return false;
				}
			}

			return true;
		}

		private static bool validateLengthMinVersusTimes(TimeSpan? minStart, TimeSpan? maxEnd,  TimeSpan? minLength)
		{
			if (minLength != null)
			{
				if (minStart != null && maxEnd != null)
				{
					var maximumLength = maxEnd.Value.Subtract(minStart.Value);

					if (minLength > maximumLength)
					{
						return false;
					}
				}
			}

			return true;
		}

		private static bool validateLengthMaxVersusTimes(TimeSpan? maxStart, TimeSpan? minEnd,TimeSpan? maxLength)
		{
			if (maxLength != null)
			{
				if (maxStart != null && minEnd != null)
				{
					var minumumLength = minEnd.Value.Subtract(maxStart.Value);
					{
						if (maxLength < minumumLength)
						{
							return false;
						}
					}
				}
			}

			return true;
		}

		private static AgentPreferenceCanCreateResult validTypes(IShiftCategory shiftCategory, IAbsence absence,IDayOffTemplate dayOffTemplate, IActivity activity, AgentPreferenceCanCreateResult result)
		{

			if (shiftCategory != null)
			{
				if (absence != null || dayOffTemplate != null)
				{
					result.Result = false;
					result.ConflictingTypeError = true;
					return result;
				}
			}
	
			if (absence != null)
			{
				if (dayOffTemplate != null || activity != null)
				{
					result.Result = false;
					result.ConflictingTypeError = true;
					return result;
				}
			}

			if (dayOffTemplate != null)
			{
				if(activity != null)
				{
					result.Result = false;
					result.ConflictingTypeError = true;
					return result;
				}
			}

			return result;
		}
	}
}
