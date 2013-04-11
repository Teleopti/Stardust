using System;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface  IAgentPreferenceDayCreator
	{
		IPreferenceDay Create(IScheduleDay scheduleDay, IShiftCategory shiftCategory, IAbsence absence,
		                      IDayOffTemplate dayOffTemplate, IActivity activity, TimeSpan? minStart, TimeSpan? maxStart,
		                      TimeSpan? minEnd, TimeSpan? maxEnd, TimeSpan? minLength, TimeSpan? maxLength,
		                      TimeSpan? minStartActivity, TimeSpan? maxStartActivity, TimeSpan? minEndActivity,
		                      TimeSpan? maxEndActivity, TimeSpan? minLengthActivity, TimeSpan? maxLengthActivity);

		IAgentPreferenceCanCreateResult CanCreate(IShiftCategory shiftCategory, IAbsence absence,
		                                          IDayOffTemplate dayOffTemplate, IActivity activity, TimeSpan? minStart,
		                                          TimeSpan? maxStart, TimeSpan? minEnd, TimeSpan? maxEnd, TimeSpan? minLength, TimeSpan? maxLength,
		                                          TimeSpan? minStartActivity, TimeSpan? maxStartActivity,
		                                          TimeSpan? minEndActivity, TimeSpan? maxEndActivity,
		                                          TimeSpan? minLengthActivity, TimeSpan? maxLengthActivity);
	}

	public class AgentPreferenceDayCreator : IAgentPreferenceDayCreator
	{
		public IPreferenceDay Create(IScheduleDay scheduleDay, IShiftCategory shiftCategory, IAbsence absence, IDayOffTemplate dayOffTemplate, IActivity activity, TimeSpan? minStart, TimeSpan? maxStart, 
									TimeSpan? minEnd, TimeSpan? maxEnd, TimeSpan? minLength, TimeSpan? maxLength,
									TimeSpan? minStartActivity, TimeSpan? maxStartActivity, TimeSpan? minEndActivity, TimeSpan? maxEndActivity, 
									TimeSpan? minLengthActivity, TimeSpan? maxLengthActivity)

		{	
			if(scheduleDay == null) throw new ArgumentNullException("scheduleDay");

			var result = CanCreate(shiftCategory, absence, dayOffTemplate, activity, minStart, maxStart, minEnd, maxEnd, minLength, maxLength, minStartActivity, maxStartActivity, minEndActivity, maxEndActivity, minLengthActivity, maxLengthActivity);
			if (!result.Result) return null;
	
			var restriction = new PreferenceRestriction();

			restriction.ShiftCategory = shiftCategory;
			restriction.Absence = absence;
			restriction.DayOffTemplate = dayOffTemplate;

			if (activity != null)
			{
				var activityRestriction = new ActivityRestriction(activity);
				activityRestriction.StartTimeLimitation = new StartTimeLimitation(minStartActivity, maxStartActivity);
				activityRestriction.EndTimeLimitation = new EndTimeLimitation(minEndActivity, maxEndActivity);
				activityRestriction.WorkTimeLimitation = new WorkTimeLimitation(minLengthActivity, maxLengthActivity);
				restriction.AddActivityRestriction(activityRestriction);
			}

			if (absence == null && dayOffTemplate == null)
			{
				restriction.StartTimeLimitation = new StartTimeLimitation(minStart, maxStart);
				restriction.EndTimeLimitation = new EndTimeLimitation(minEnd, maxEnd);
				restriction.WorkTimeLimitation = new WorkTimeLimitation(minLength, maxLength);
			}

			var preferenceDay = new PreferenceDay(scheduleDay.Person, scheduleDay.DateOnlyAsPeriod.DateOnly, restriction);

			return preferenceDay;
		}

		public IAgentPreferenceCanCreateResult CanCreate(IShiftCategory shiftCategory, IAbsence absence,
												  IDayOffTemplate dayOffTemplate, IActivity activity, TimeSpan? minStart,
												  TimeSpan? maxStart, TimeSpan? minEnd, TimeSpan? maxEnd, TimeSpan? minLength, TimeSpan? maxLength,
												  TimeSpan? minStartActivity, TimeSpan? maxStartActivity,
												  TimeSpan? minEndActivity, TimeSpan? maxEndActivity,
												  TimeSpan? minLengthActivity, TimeSpan? maxLengthActivity)
		{
			var result = new AgentPreferenceCanCreateResult();
			result.Result = true;

			if (validateEmpty(shiftCategory, absence, dayOffTemplate, activity, minStart, maxStart, minEnd, maxEnd, minLength,
			                  maxLength, minStartActivity, maxStartActivity, minEndActivity, maxEndActivity, minLengthActivity,
			                  maxLengthActivity))
			{
				result.Result = false;
				result.EmptyError = true;
				return result;
			}

			//result = validateStartTimes(minStart, maxStart, result);
			//if (!result.Result) return result;
			if (!validateStartTimes(minStart, maxStart, result))
			{
				result.Result = false;
				result.StartTimeMinError = true;
				//return result;
			}

			if (!validateStartTimes(minStartActivity, maxStartActivity, result))
			{
				result.Result = false;
				result.StartTimeMinErrorActivity = true;
				//return result;
			}

			if (!validateEndTimes(minEnd, maxEnd, result))
			{
				result.Result = false;
				result.EndTimeMinError = true;
				//return result;
			}

			if (!validateEndTimes(minEndActivity, maxEndActivity, result))
			{
				result.Result = false;
				result.EndTimeMinErrorActivity = true;
				//return result;
			}

			if (!validateLengths(minLength, maxLength, result))
			{
				result.Result = false;
				result.LengthMinError = true;
				//return result;
			}

			if (!validateLengths(minLengthActivity, maxLengthActivity, result))
			{
				result.Result = false;
				result.LengthMinErrorActivity = true;
				//return result;
			}

			if (!validateStartTimesMinVersusEndTimes(minStart, maxStart, minEnd, maxEnd, result))
			{
				result.Result = false;
				result.StartTimeMinError = true;
				//return result;
			}

			if (!validateStartTimesMinVersusEndTimes(minStartActivity, maxStartActivity, minEndActivity, maxEndActivity, result))
			{
				result.Result = false;
				result.StartTimeMinErrorActivity = true;
				//return result;
			}

			if (!validateStartTimesMaxVersusEndTimes(minStart, maxStart, minEnd, maxEnd, result))
			{
				result.Result = false;
				result.StartTimeMaxError = true;
				//return result;
			}

			if (!validateStartTimesMaxVersusEndTimes(minStartActivity, maxStartActivity, minEndActivity, maxEndActivity, result))
			{
				result.Result = false;
				result.StartTimeMaxErrorActivity = true;
				//return result;
			}

			if (!validateLengthMinVersusTimes(minStart, maxStart, minEnd, maxEnd, minLength, maxLength, result))
			{
				result.Result = false;
				result.LengthMinError = true;
				//return result;
			}

			if (!validateLengthMinVersusTimes(minStartActivity, maxStartActivity, minEndActivity, maxEndActivity, minLengthActivity, maxLengthActivity, result))
			{
				result.Result = false;
				result.LengthMinErrorActivity = true;
				//return result;
			}

			if (!validateLengthMaxVersusTimes(minStart, maxStart, minEnd, maxEnd, minLength, maxLength, result))
			{
				result.Result = false;
				result.LengthMaxError = true;
				//return result;
			}

			if (!validateLengthMaxVersusTimes(minStartActivity, maxStartActivity, minEndActivity, maxEndActivity, minLengthActivity, maxLengthActivity, result))
			{
				result.Result = false;
				result.LengthMaxErrorActivity = true;
				//return result;
			}

			result = validTypes(shiftCategory, absence, dayOffTemplate, activity, result);
			//if (!result.Result) return result;

			return result;
		}

		private bool validateEmpty(IShiftCategory shiftCategory, IAbsence absence,
		                           IDayOffTemplate dayOffTemplate, IActivity activity, TimeSpan? minStart,
		                           TimeSpan? maxStart, TimeSpan? minEnd, TimeSpan? maxEnd, TimeSpan? minLength,
		                           TimeSpan? maxLength,
		                           TimeSpan? minStartActivity, TimeSpan? maxStartActivity,
		                           TimeSpan? minEndActivity, TimeSpan? maxEndActivity,
		                           TimeSpan? minLengthActivity, TimeSpan? maxLengthActivity)
		{
			if (shiftCategory == null && absence == null && dayOffTemplate == null && activity == null && minStart == null
			    && maxStart == null && minEnd == null && maxEnd == null && minLength == null && maxLength == null &&
			    minStartActivity == null
			    && maxStartActivity == null && minEndActivity == null && maxEndActivity == null && minLengthActivity == null &&
				maxLengthActivity == null) return true;

				return false;
		
		}

		private static bool validateStartTimes(TimeSpan? minStart, TimeSpan? maxStart, AgentPreferenceCanCreateResult result)
		{
			if (minStart != null && maxStart != null)
			{
				if (minStart.Value > maxStart.Value)
				{
					//result.Result = false;
					//result.StartTimeMinError = true;
					return false;
				}
			}

			//return result;
			return true;
		}


		private static bool validateEndTimes(TimeSpan? minEnd, TimeSpan? maxEnd, AgentPreferenceCanCreateResult result)
		{
			if (minEnd != null && maxEnd != null)
			{
				if (minEnd.Value > maxEnd.Value)
				{
					//result.Result = false;
					//result.EndTimeMinError = true;
					return false;
				}
			}

			return true;
		}

		
		private static bool validateLengths(TimeSpan? minLength, TimeSpan? maxLength, AgentPreferenceCanCreateResult result)
		{
			if (minLength != null && maxLength != null)
			{
				if (minLength.Value > maxLength.Value)
				{
					//result.Result = false;
					//result.LengthMinError = true;
					return false;
				}
			}

			return true;
		}

		private static bool validateStartTimesMinVersusEndTimes(TimeSpan? minStart, TimeSpan? maxStart, TimeSpan? minEnd, TimeSpan? maxEnd, AgentPreferenceCanCreateResult result)
		{
			if (minStart != null)
			{
				if (minEnd != null && minStart.Value >= minEnd.Value)
				{
					//result.Result = false;
					//result.StartTimeMinError = true;
					return false;
				}

				if (maxEnd != null && minStart.Value >= maxEnd.Value)
				{
					//result.Result = false;
					//result.StartTimeMinError = true;
					return false;
				}
			}


			return true;
		}

		private static bool validateStartTimesMaxVersusEndTimes(TimeSpan? minStart, TimeSpan? maxStart, TimeSpan? minEnd, TimeSpan? maxEnd, AgentPreferenceCanCreateResult result)
		{
			if (maxStart != null)
			{
				if (minEnd != null && maxStart.Value >= minEnd.Value)
				{
					//result.Result = false;
					//result.StartTimeMaxError = true;
					return false;
				}

				if (maxEnd != null && maxStart.Value >= maxEnd.Value)
				{
					//result.Result = false;
					//result.StartTimeMaxError = true;
					return false;
				}
			}

			return true;
		}

		private static bool validateLengthMinVersusTimes(TimeSpan? minStart, TimeSpan? maxStart, TimeSpan? minEnd, TimeSpan? maxEnd,  TimeSpan? minLength,  TimeSpan? maxLength, AgentPreferenceCanCreateResult result)
		{
			if (minLength != null)
			{
				if (minStart != null && maxEnd != null)
				{
					var maximumLength = maxEnd.Value.Subtract(minStart.Value);

					if (minLength > maximumLength)
					{
						//result.Result = false;
						//result.LengthMinError = true;
						return false;
					}
				}
			}

			//if (maxLength != null)
			//{
			//	if (maxStart != null && minEnd != null)
			//	{
			//		var minumumLength = minEnd.Value.Subtract(maxStart.Value);
			//		{
			//			if (maxLength < minumumLength)
			//			{
			//				result.Result = false;
			//				result.LengthMaxError = true;
			//				return result;
			//			}
			//		}
			//	}
			//}

			return true;
		}

		private static bool validateLengthMaxVersusTimes(TimeSpan? minStart, TimeSpan? maxStart, TimeSpan? minEnd, TimeSpan? maxEnd, TimeSpan? minLength, TimeSpan? maxLength, AgentPreferenceCanCreateResult result)
		{
			//if (minLength != null)
			//{
			//	if (minStart != null && maxEnd != null)
			//	{
			//		var maximumLength = maxEnd.Value.Subtract(minStart.Value);

			//		if (minLength > maximumLength)
			//		{
			//			result.Result = false;
			//			result.LengthMinError = true;
			//			return result;
			//		}
			//	}
			//}

			if (maxLength != null)
			{
				if (maxStart != null && minEnd != null)
				{
					var minumumLength = minEnd.Value.Subtract(maxStart.Value);
					{
						if (maxLength < minumumLength)
						{
							//result.Result = false;
							//result.LengthMaxError = true;
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
