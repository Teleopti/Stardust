using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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

			var maximumContinuousWorkTime = person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime;
			var timezone = person.PermissionInformation.DefaultTimeZone();
			var dic = getScheduleDictionary(context.PersonRequest.Request.Period.ToDateOnlyPeriod(timezone), person);

			var requestPeriod = context.PersonRequest.Request.Period;
			var personAssignment = dic[person].ScheduledDay(new DateOnly(requestPeriod.StartDateTimeLocal(timezone))).PersonAssignment();

			if (personAssignment == null)
			{
				personAssignment = dic[person].ScheduledDay(new DateOnly(requestPeriod.StartDateTimeLocal(timezone)).AddDays(-1))
					.PersonAssignment();
			}

			var continuousWorkTimePeriod = string.Empty;
			var continuousWorkTime = TimeSpan.Zero;

			if (personAssignment != null)
			{
				var previousShiftLayer = personAssignment.ShiftLayers.FirstOrDefault(shift =>
					shift.Period.EndDateTime.CompareTo(requestPeriod.StartDateTime) <= 0);
				var nextShiftLayer = personAssignment.ShiftLayers.FirstOrDefault(shift =>
					shift.Period.StartDateTime.CompareTo(requestPeriod.EndDateTime) >= 0);

				if (previousShiftLayer != null)
				{
					continuousWorkTime = previousShiftLayer.Period.ElapsedTime() + requestPeriod.ElapsedTime();
					continuousWorkTimePeriod =
						$"{previousShiftLayer.Period.StartDateTimeLocal(timezone)} - {requestPeriod.EndDateTimeLocal(timezone)}";
				}

				if (nextShiftLayer != null)
				{
					continuousWorkTime = nextShiftLayer.Period.ElapsedTime() + requestPeriod.ElapsedTime();
					continuousWorkTimePeriod =
						$"{requestPeriod.StartDateTimeLocal(timezone)} - {nextShiftLayer.Period.EndDateTimeLocal(timezone)}";
				}

				if (previousShiftLayer == null && nextShiftLayer == null)
				{
					continuousWorkTime = requestPeriod.ElapsedTime();
					continuousWorkTimePeriod =
						$"{requestPeriod.StartDateTimeLocal(timezone)} - {requestPeriod.EndDateTimeLocal(timezone)}";
				}
			}
			else
			{
				continuousWorkTime = requestPeriod.ElapsedTime();
				continuousWorkTimePeriod =
					$"{requestPeriod.StartDateTimeLocal(timezone)} - {requestPeriod.EndDateTimeLocal(timezone)}";

				if (requestPeriod.EndDateTimeLocal(timezone).Date.CompareTo(requestPeriod.StartDateTimeLocal(timezone).Date) > 0)
				{
					personAssignment = dic[person].ScheduledDay(new DateOnly(requestPeriod.StartDateTimeLocal(timezone)).AddDays(1))
						.PersonAssignment();

					var nextShiftLayer = personAssignment.ShiftLayers.FirstOrDefault(shift =>
						shift.Period.StartDateTime.CompareTo(requestPeriod.EndDateTime) >= 0);

					if (nextShiftLayer != null)
					{
						continuousWorkTime = nextShiftLayer.Period.ElapsedTime() + requestPeriod.ElapsedTime();
						continuousWorkTimePeriod =
							$"{requestPeriod.StartDateTimeLocal(timezone)} - {nextShiftLayer.Period.EndDateTimeLocal(timezone)}";
					}
				}
			}


			if (continuousWorkTime.CompareTo(maximumContinuousWorkTime) <= 0)
			{
				return validResult();
			}

			return new OvertimeRequestValidationResult
			{
				InvalidReasons = new[]
				{
					string.Format(Resources.OvertimeRequestContinuousWorkTimeDenyReason,
						continuousWorkTimePeriod,
						DateHelper.HourMinutesString(continuousWorkTime.TotalMinutes),
						DateHelper.HourMinutesString(maximumContinuousWorkTime.Value.TotalMinutes))
				}
			};
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
	}
}