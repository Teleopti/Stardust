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
			var schedulePeriod = context.PersonRequest.Request.Period.ToDateOnlyPeriod(timezone).Inflate(1);

			var dic = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, false),
				schedulePeriod, _currentScenario.Current());

			var requestPeriod = context.PersonRequest.Request.Period;
			var scheduleDay = dic[person].ScheduledDay(new DateOnly(requestPeriod.StartDateTimeLocal(timezone)));
			var personAssignment = scheduleDay.PersonAssignment();

			var lastShiftLayer = personAssignment.ShiftLayers
				.FirstOrDefault(shift => shift.Period.EndDateTime.CompareTo(requestPeriod.StartDateTime) <= 0);
			var continuousWorkTimePeriod = string.Empty;
			var continuousWorkTime = TimeSpan.Zero;
			if (lastShiftLayer != null)
			{
				continuousWorkTime = lastShiftLayer.Period.ElapsedTime() + requestPeriod.ElapsedTime();
				continuousWorkTimePeriod =
					$"{lastShiftLayer.Period.StartDateTimeLocal(timezone)} - {requestPeriod.EndDateTimeLocal(timezone)}";
			}
			else
			{
				continuousWorkTime = requestPeriod.ElapsedTime();
				continuousWorkTimePeriod =
					$"{requestPeriod.StartDateTimeLocal(timezone)} - {requestPeriod.EndDateTimeLocal(timezone)}";
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

		private static OvertimeRequestValidationResult validResult()
		{
			return new OvertimeRequestValidationResult()
			{
				IsValid = true
			};
		}
	}
}