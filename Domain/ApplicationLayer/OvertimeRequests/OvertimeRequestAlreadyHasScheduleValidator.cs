using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestAlreadyHasScheduleValidator : IOvertimeRequestValidator
	{
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;
		private readonly ILoggedOnUser _loggedOnUser;

		public OvertimeRequestAlreadyHasScheduleValidator(ILoggedOnUser loggedOnUser, IScheduleStorage scheduleStorage, ICurrentScenario currentScenario)
		{
			_loggedOnUser = loggedOnUser;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
		}

		public OvertimeRequestValidationResult Validate(OvertimeRequestValidationContext context)
		{
			var personRequest = context.PersonRequest;
			var person = _loggedOnUser.CurrentUser();
			var period = personRequest.Request.Period;
			var scheduleperiod = period.ChangeStartTime(TimeSpan.FromDays(-1));
			var dic = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false),
				scheduleperiod, _currentScenario.Current());

			var agentDate = new DateOnly(TimeZoneHelper.ConvertFromUtc(period.StartDateTime, person.PermissionInformation.DefaultTimeZone()));
			var scheduleDays = dic[person].ScheduledDayCollection(new DateOnlyPeriod(agentDate.AddDays(-1),agentDate));

			var personAssignment = scheduleDays.Last().PersonAssignment();
			var personAssignmentYesterDay = scheduleDays.First().PersonAssignment();

			if (hasActivity(personAssignment, period) || hasActivity(personAssignmentYesterDay, period))
			{
				var timeZone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
				return new OvertimeRequestValidationResult
				{
					IsValid = false,
					InvalidReasons = new[]
					{
						string.Format(Resources.OvertimeRequestAlreadyHasScheduleInPeriod,
							personRequest.Request.Period.StartDateTimeLocal(timeZone),
							personRequest.Request.Period.EndDateTimeLocal(timeZone))
					}
				};
			}
			return new OvertimeRequestValidationResult { IsValid = true };
		}

		private bool hasActivity(IPersonAssignment personAssignment, DateTimePeriod period)
		{
			if (personAssignment == null)
				return false;

			var mainActivities = personAssignment.MainActivities();
			var overtimeActivities = personAssignment.OvertimeActivities();

			if (mainActivities != null && mainActivities.Any(a => !a.Payload.InContractTime && a.Period.Contains(period)))
				return false;

			if (mainActivities != null && (mainActivities.Any(a => a.Period.Intersect(period)) || overtimeActivities.Any(a => a.Period.Intersect(period))))
				return true;
			return false;
		}
	}
}