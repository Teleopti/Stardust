using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourcePlanner.Validation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class FullSchedulingResult
	{
		private readonly IFindSchedulesForPersons _findSchedulesForPersons;
		private readonly ICurrentScenario _currentScenario;
		private readonly IUserTimeZone _userTimeZone;
		private readonly SchedulingValidator _schedulingValidator;

		public FullSchedulingResult(IFindSchedulesForPersons findSchedulesForPersons, 
			ICurrentScenario currentScenario, IUserTimeZone userTimeZone, SchedulingValidator schedulingValidator)
		{
			_findSchedulesForPersons = findSchedulesForPersons;
			_currentScenario = currentScenario;
			_userTimeZone = userTimeZone;
			_schedulingValidator = schedulingValidator;
		}

		public SchedulingResultModel Execute(DateOnlyPeriod period, IEnumerable<IPerson> fixedStaffPeople)
		{
			var personsProvider = new PersonsInOrganizationProvider(fixedStaffPeople) { DoLoadByPerson = true };
			var scheduleOfSelectedPeople = _findSchedulesForPersons.FindSchedulesForPersons(new ScheduleDateTimePeriod(period.ToDateTimePeriod(_userTimeZone.TimeZone()), fixedStaffPeople), _currentScenario.Current(), personsProvider, new ScheduleDictionaryLoadOptions(false, false, false), fixedStaffPeople);

			return new SchedulingResultModel
			{
				ScheduledAgentsCount = successfulScheduledAgents(scheduleOfSelectedPeople, period),
				BusinessRulesValidationResults = _schedulingValidator.Validate(scheduleOfSelectedPeople, fixedStaffPeople, period).InvalidResources
			};
		}

		//move to seperate class
		private static int successfulScheduledAgents(IEnumerable<KeyValuePair<IPerson, IScheduleRange>> schedules, DateOnlyPeriod periodToCheck)
		{
			return schedules.Count(x =>
			{
				var targetSummary = x.Value.CalculatedTargetTimeSummary(periodToCheck);
				var scheduleSummary = x.Value.CalculatedCurrentScheduleSummary(periodToCheck);
				return targetSummary.TargetTime.HasValue &&
					   targetSummary.TargetTime - targetSummary.NegativeTargetTimeTolerance <= scheduleSummary.ContractTime &&
					   targetSummary.TargetTime + targetSummary.PositiveTargetTimeTolerance >= scheduleSummary.ContractTime &&
				   		x.Value.CalculatedCurrentScheduleSummary(periodToCheck).DaysWithoutSchedule == 0;
			});
		}
	}
}