using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public class CreatePersonalSkillDataExtractor : ICreatePersonalSkillDataExtractor
	{
		private readonly PersonalSkillsProvider _personalSkillsProvider;
		private readonly ISkillPriorityProvider _skillPriorityProvider;
		private readonly IUserTimeZone _userTimeZone;

		public CreatePersonalSkillDataExtractor(PersonalSkillsProvider personalSkillsProvider, ISkillPriorityProvider skillPriorityProvider, IUserTimeZone userTimeZone)
		{
			_personalSkillsProvider = personalSkillsProvider;
			_skillPriorityProvider = skillPriorityProvider;
			_userTimeZone = userTimeZone;
		}
		
		public IScheduleResultDataExtractor Create(IScheduleMatrixPro scheduleMatrix, IOptimizationPreferences optimizationPreferences, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			ISkillExtractor skillExtractor = new ScheduleMatrixPersonalSkillExtractor(scheduleMatrix, _personalSkillsProvider);
			if (optimizationPreferences.Advanced.UseMinimumStaffing)
			{
				var dailySkillForecastAndScheduledValueCalculator = new DailyBoostedSkillForecastAndScheduledValueCalculator(()=> schedulingResultStateHolder, _skillPriorityProvider, _userTimeZone);
				return new RelativeBoostedDailyDifferencesByPersonalSkillsExtractor(scheduleMatrix, dailySkillForecastAndScheduledValueCalculator, skillExtractor);
			}
			else
			{
				var dailySkillForecastAndScheduledValueCalculator = new DailySkillForecastAndScheduledValueCalculator(()=> schedulingResultStateHolder, _userTimeZone);
				return new RelativeDailyDifferencesByPersonalSkillsExtractor(scheduleMatrix, dailySkillForecastAndScheduledValueCalculator, skillExtractor);
			}
		}
	}
}