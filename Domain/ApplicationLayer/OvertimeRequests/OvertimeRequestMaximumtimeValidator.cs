using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestMaximumtimeValidator : IOvertimeRequestValidator
	{
		private readonly ICurrentScenario _currentScenario;
		private readonly IScheduleStorage _scheduleStorage;

		public OvertimeRequestMaximumtimeValidator(IScheduleStorage scheduleStorage, ICurrentScenario currentScenario)
		{
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
		}

		public OvertimeRequestValidationResult Validate(OvertimeRequestValidationContext context)
		{
			var workflowControlSet = context.PersonRequest.Person.WorkflowControlSet;
			if (workflowControlSet.OvertimeRequestMaximumTime == null ||
				workflowControlSet.OvertimeRequestMaximumTime == TimeSpan.Zero)
			{
				return new OvertimeRequestValidationResult
				{
					IsValid = true
				};
			}

			var maximumTime = workflowControlSet.OvertimeRequestMaximumTime;
			var calendarMonthOvertimes = getCalendarMonthOvertimes(context.PersonRequest.Request);
			var culture = context.PersonRequest.Person.PermissionInformation.Culture();
			var invalidReasons = new List<string>();
			foreach (var calendarMonthOvertime in calendarMonthOvertimes)
			{
				if (calendarMonthOvertime.Value > maximumTime)
				{
					invalidReasons.Add(string.Format(Resources.OvertimeRequestMaximumTimeDenyReason,
						DateHelper.GetMonthName(calendarMonthOvertime.Key.Date, culture),
						DateHelper.HourMinutesString(calendarMonthOvertime.Value.TotalMinutes),
						DateHelper.HourMinutesString(maximumTime.Value.TotalMinutes)));
				}
			}

			if (invalidReasons.Count > 0)
			{
				return new OvertimeRequestValidationResult
				{
					IsValid = false,
					ShouldDenyIfInValid = workflowControlSet.OvertimeRequestMaximumTimeHandleType == OvertimeValidationHandleType.Deny,
					InvalidReasons = invalidReasons.ToArray()
				};
			}

			return new OvertimeRequestValidationResult
			{
				IsValid = true
			};
		}

		private Dictionary<DateOnly, TimeSpan> getCalendarMonthOvertimes(IRequest request)
		{
			var result = new Dictionary<DateOnly, TimeSpan>();

			var person = request.Person;
			var culture = person.PermissionInformation.Culture();
			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var requestPeriod = request.Period.ToDateOnlyPeriod(timeZone);

			var dic = getScheduleDictionary(requestPeriod, culture, person);

			var localStartTime = request.Period.StartDateTimeLocal(timeZone);
			var localEndTime = request.Period.EndDateTimeLocal(timeZone);

			var firstMonthSchedulePeriod = new DateOnlyPeriod(
				new DateOnly(DateHelper.GetFirstDateInMonth(localStartTime, culture)),
				new DateOnly(DateHelper.GetLastDateInMonth(localStartTime, culture)));
			result.Add(firstMonthSchedulePeriod.StartDate, getOvertimeInSchedule(dic[person], firstMonthSchedulePeriod));

			if (!isCrossMonth(requestPeriod))
			{
				result[firstMonthSchedulePeriod.StartDate] += request.Period.ElapsedTime();
			}
			else
			{
				var firstMonthRequestOverTime = DateHelper.GetFirstDateInMonth(localEndTime, culture) - localStartTime;
				result[firstMonthSchedulePeriod.StartDate] += firstMonthRequestOverTime;

				var secondMonthSchedulePeriod = new DateOnlyPeriod(
					new DateOnly(DateHelper.GetFirstDateInMonth(localEndTime, culture)),
					new DateOnly(DateHelper.GetLastDateInMonth(localEndTime, culture)));
				var secondMonthRequestOverTime = localEndTime - DateHelper.GetFirstDateInMonth(localEndTime, culture);

				result.Add(secondMonthSchedulePeriod.StartDate,
					secondMonthRequestOverTime + getOvertimeInSchedule(dic[person], secondMonthSchedulePeriod));
			}

			return result;
		}

		private IScheduleDictionary getScheduleDictionary(DateOnlyPeriod requestPeriod, CultureInfo culture, IPerson person)
		{
			DateOnlyPeriod schedulePeriod;
			if (!isCrossMonth(requestPeriod))
			{
				schedulePeriod = new DateOnlyPeriod(
					new DateOnly(DateHelper.GetFirstDateInMonth(requestPeriod.StartDate.Date, culture)),
					new DateOnly(DateHelper.GetLastDateInMonth(requestPeriod.StartDate.Date, culture)));
			}
			else
			{
				schedulePeriod = new DateOnlyPeriod(
					new DateOnly(DateHelper.GetFirstDateInMonth(requestPeriod.StartDate.Date, culture)),
					new DateOnly(DateHelper.GetLastDateInMonth(requestPeriod.EndDate.Date, culture)));
			}

			var dic = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, false),
				schedulePeriod, _currentScenario.Current());
			return dic;
		}

		private TimeSpan getOvertimeInSchedule(IScheduleRange scheduleRange, DateOnlyPeriod period)
		{
			var totalTime = TimeSpan.Zero;
			var schedules = scheduleRange.ScheduledDayCollection(period);
			foreach (IScheduleDay scheduleDay in schedules)
			{
				var projSvc = scheduleDay.ProjectionService();
				var visualLayerCollection = projSvc.CreateProjection();
				totalTime += visualLayerCollection.Overtime();
			}
			return totalTime;
		}

		private bool isCrossMonth(DateOnlyPeriod requestPeriod)
		{
			return requestPeriod.StartDate.Month != requestPeriod.EndDate.Month;
		}
	}
}