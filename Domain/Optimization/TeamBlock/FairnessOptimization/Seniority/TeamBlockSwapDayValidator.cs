using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	public interface ITeamBlockSwapDayValidator
	{
		bool ValidateSwapDays(IScheduleDay scheduleDay1, IScheduleDay scheduleDay2);
	}
    
	public class TeamBlockSwapDayValidator : ITeamBlockSwapDayValidator
	{
		private readonly ITeamBlockPersonsSkillChecker _teamBlockPersonsSkillChecker;

		public TeamBlockSwapDayValidator(ITeamBlockPersonsSkillChecker teamBlockPersonsSkillChecker)
		{
			_teamBlockPersonsSkillChecker = teamBlockPersonsSkillChecker;
		}

		public bool ValidateSwapDays(IScheduleDay scheduleDay1, IScheduleDay scheduleDay2)
		{
			var personPeriod1 = scheduleDay1.Person.Period(scheduleDay1.DateOnlyAsPeriod.DateOnly);
			var personPeriod2 = scheduleDay2.Person.Period(scheduleDay2.DateOnlyAsPeriod.DateOnly);

			var result = _teamBlockPersonsSkillChecker.PersonsHaveSameSkills(personPeriod1, personPeriod2);
			if (!result) return false;

			result = personPeriod1.RuleSetBag.Equals(personPeriod2.RuleSetBag);
			if (!result) return false;

			result = scheduleDay1.ProjectionService().CreateProjection().ContractTime().Equals(scheduleDay2.ProjectionService().CreateProjection().ContractTime());

			return result;
		}
	}
}
