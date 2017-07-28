using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

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

		public OvertimeRequestValidationResult Validate(IPersonRequest personRequest)
		{
			var person = _loggedOnUser.CurrentUser();
			var period = personRequest.Request.Period;
			var dic = _scheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(period), _currentScenario.Current(),
							new PersonProvider(new[] { person }), new ScheduleDictionaryLoadOptions(false, false), new[] { person });

			var agentDate = TimeZoneHelper.ConvertFromUtc(period.StartDateTime, person.PermissionInformation.DefaultTimeZone());
			var scheduleDay = dic[person].ScheduledDay(new DateOnly(agentDate));

			var personAssignment = scheduleDay.PersonAssignment();
			if (personAssignment == null) return new OvertimeRequestValidationResult { IsValid = true };

			var mainActivities = personAssignment.MainActivities();
			var overtimeActivities = personAssignment.OvertimeActivities();

			if( mainActivities.Any(a => !a.Payload.InContractTime && a.Period.Contains(period))) return new OvertimeRequestValidationResult { IsValid = true };

			var hasWorkingSchedule = mainActivities.Any(a => a.Period.Intersect(period)) || overtimeActivities.Any(a => a.Period.Intersect(period));
			if (!hasWorkingSchedule) return new OvertimeRequestValidationResult { IsValid = true };

			return new OvertimeRequestValidationResult
			{
				IsValid = false,
				InvalidReason = string.Format(Resources.OvertimeRequestAlreadyHasScheduleInPeriod,
						TimeZoneHelper.ConvertFromUtc(personRequest.Request.Period.StartDateTime, _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone()),
						TimeZoneHelper.ConvertFromUtc(personRequest.Request.Period.EndDateTime, _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone()))
			};
		}
	}
}