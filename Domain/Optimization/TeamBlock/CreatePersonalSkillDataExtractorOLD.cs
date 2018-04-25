using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MinimumStaffing_75339)]
	public class CreatePersonalSkillDataExtractorOLD : ICreatePersonalSkillDataExtractor
	{
		private readonly IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;
		private readonly IUserTimeZone _userTimeZone;

		public CreatePersonalSkillDataExtractorOLD(IScheduleResultDataExtractorProvider scheduleResultDataExtractorProvider, IUserTimeZone userTimeZone)
		{
			_scheduleResultDataExtractorProvider = scheduleResultDataExtractorProvider;
			_userTimeZone = userTimeZone;
		}
		
		public IScheduleResultDataExtractor Create(IScheduleMatrixPro scheduleMatrix, IOptimizationPreferences optimizationPreferences,
			ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			return _scheduleResultDataExtractorProvider.CreatePersonalSkillDataExtractor(scheduleMatrix,
				optimizationPreferences.Advanced, schedulingResultStateHolder);
		}

		public IDailySkillForecastAndScheduledValueCalculator CreateCalculator(IOptimizationPreferences optimizationPreferences,
			ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			return new DailySkillForecastAndScheduledValueCalculator(()=> schedulingResultStateHolder, _userTimeZone);
		}
	}
	
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MinimumStaffing_75339)]
	public interface ICreatePersonalSkillDataExtractor
	{
		IScheduleResultDataExtractor Create(IScheduleMatrixPro scheduleMatrix, IOptimizationPreferences optimizationPreferences,
			ISchedulingResultStateHolder schedulingResultStateHolder);

		IDailySkillForecastAndScheduledValueCalculator CreateCalculator(IOptimizationPreferences optimizationPreferences, ISchedulingResultStateHolder schedulingResultStateHolder);
	}
}