using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestMaximumContinuousWorkTimeValidator : IOvertimeRequestValidator
	{
		private readonly ICurrentScenario _currentScenario;
		private readonly IScheduleStorage _scheduleStorage;

		public OvertimeRequestMaximumContinuousWorkTimeValidator(ICurrentScenario currentScenario, IScheduleStorage scheduleStorage)
		{
			_currentScenario = currentScenario;
			_scheduleStorage = scheduleStorage;
		}

		public OvertimeRequestValidationResult Validate(OvertimeRequestValidationContext context)
		{
			var person = context.PersonRequest.Person;
			var enableMaximumContinuousWorkTimeCheck = person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled;

			if (!enableMaximumContinuousWorkTimeCheck)
			{
				return validResult();
			}

			var maximumContinuousWorkTime =
				person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime.GetValueOrDefault(TimeSpan.Zero);
			var timezone = person.PermissionInformation.DefaultTimeZone();
			var dic = getScheduleDictionary(context.PersonRequest.Request.Period.ToDateOnlyPeriod(timezone), person);
			var scheduleRange = dic[person];
			var requestPeriod = context.PersonRequest.Request.Period;

			var shiftLayers = loadShiftLayers(scheduleRange, requestPeriod, timezone);

			var continuousWorkTimeInfo = getContinuousWorkTime(shiftLayers, requestPeriod, timezone);
			if (!isSatisfiedMaximumContinuousWorkTime(continuousWorkTimeInfo, maximumContinuousWorkTime))
			{
				return invalidResult(continuousWorkTimeInfo.ContinuousWorkTimePeriod,
					continuousWorkTimeInfo.ContinuousWorkTime,
					maximumContinuousWorkTime);
			}

			return validResult();
		}

		private static OvertimeRequestValidationResult invalidResult(string continuousWorkTimePeriod,
			TimeSpan continuousWorkTime, TimeSpan maximumContinuousWorkTime)
		{
			return new OvertimeRequestValidationResult
			{
				InvalidReasons = new[]
				{
					string.Format(Resources.OvertimeRequestContinuousWorkTimeDenyReason,
						continuousWorkTimePeriod,
						DateHelper.HourMinutesString(continuousWorkTime.TotalMinutes),
						DateHelper.HourMinutesString(maximumContinuousWorkTime.TotalMinutes))
				}
			};
		}

		private List<ShiftLayer> loadShiftLayers(IScheduleRange scheduleRange, DateTimePeriod requestPeriod,
			TimeZoneInfo timezone)
		{
			var shiftLayers = new List<ShiftLayer>();
			var today = new DateOnly(requestPeriod.StartDateTimeLocal(timezone));

			var yesterDayPersonAssignment = scheduleRange.ScheduledDay(today.AddDays(-1)).PersonAssignment();
			if (yesterDayPersonAssignment != null)
				shiftLayers.AddRange(yesterDayPersonAssignment.ShiftLayers);

			var todayDayPersonAssignment = scheduleRange.ScheduledDay(today).PersonAssignment();
			if (todayDayPersonAssignment != null)
				shiftLayers.AddRange(todayDayPersonAssignment.ShiftLayers);

			var tomorrowPersonAssignment = scheduleRange.ScheduledDay(today.AddDays(1)).PersonAssignment();
			if (tomorrowPersonAssignment != null)
				shiftLayers.AddRange(tomorrowPersonAssignment.ShiftLayers);

			return shiftLayers.OrderBy(shift => shift.Period.StartDateTime).ToList();
		}

		private ContinuousWorkTimeInfo getContinuousWorkTime(List<ShiftLayer> shiftLayers,
			DateTimePeriod requestPeriod,
			TimeZoneInfo timezone)
		{
			if (!shiftLayers.Any())
			{
				return new ContinuousWorkTimeInfo
				{
					ContinuousWorkTime = requestPeriod.ElapsedTime(),
					ContinuousWorkTimePeriod =
						$"{requestPeriod.StartDateTimeLocal(timezone)} - {requestPeriod.EndDateTimeLocal(timezone)}"
				};
			}

			string continuousWorkTimePeriod;
			TimeSpan continuousWorkTime;
			var previousShiftLayer = shiftLayers.FirstOrDefault(shift =>
				shift.Period.EndDateTime.CompareTo(requestPeriod.StartDateTime) <= 0);
			var nextShiftLayer = shiftLayers.FirstOrDefault(shift =>
				shift.Period.StartDateTime.CompareTo(requestPeriod.EndDateTime) >= 0);

			if (previousShiftLayer != null && nextShiftLayer != null)
			{
				continuousWorkTime = previousShiftLayer.Period.ElapsedTime() + requestPeriod.ElapsedTime() +
									 nextShiftLayer.Period.ElapsedTime();
				continuousWorkTimePeriod =
					$"{previousShiftLayer.Period.StartDateTimeLocal(timezone)} - {nextShiftLayer.Period.EndDateTimeLocal(timezone)}";
			}
			else if (previousShiftLayer != null)
			{
				continuousWorkTime = previousShiftLayer.Period.ElapsedTime() + requestPeriod.ElapsedTime();
				continuousWorkTimePeriod =
					$"{previousShiftLayer.Period.StartDateTimeLocal(timezone)} - {requestPeriod.EndDateTimeLocal(timezone)}";
			}
			else if (nextShiftLayer != null)
			{
				continuousWorkTime = nextShiftLayer.Period.ElapsedTime() + requestPeriod.ElapsedTime();
				continuousWorkTimePeriod =
					$"{requestPeriod.StartDateTimeLocal(timezone)} - {nextShiftLayer.Period.EndDateTimeLocal(timezone)}";
			}
			else
			{
				continuousWorkTime = requestPeriod.ElapsedTime();
				continuousWorkTimePeriod =
					$"{requestPeriod.StartDateTimeLocal(timezone)} - {requestPeriod.EndDateTimeLocal(timezone)}";
			}

			return new ContinuousWorkTimeInfo
			{
				ContinuousWorkTime = continuousWorkTime,
				ContinuousWorkTimePeriod = continuousWorkTimePeriod
			};
		}

		private bool isSatisfiedMaximumContinuousWorkTime(ContinuousWorkTimeInfo continuousWorkTimeInfo,
			TimeSpan maximumContinuousWorkTime)
		{
			return continuousWorkTimeInfo.ContinuousWorkTime.CompareTo(maximumContinuousWorkTime) <= 0;
		}

		private IScheduleDictionary getScheduleDictionary(DateOnlyPeriod period, IPerson person)
		{
			var schedulePeriod = period.Inflate(1);

			var dic = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, false),
				schedulePeriod, _currentScenario.Current());

			return dic;
		}

		private static OvertimeRequestValidationResult validResult()
		{
			return new OvertimeRequestValidationResult()
			{
				IsValid = true
			};
		}

		private class ContinuousWorkTimeInfo
		{
			public TimeSpan ContinuousWorkTime { get; set; }

			public string ContinuousWorkTimePeriod { get; set; }
		}
	}
}