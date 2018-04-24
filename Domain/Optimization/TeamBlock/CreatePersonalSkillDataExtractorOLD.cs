using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MinimumStaffing_75339)]
	public class CreatePersonalSkillDataExtractorOLD : ICreatePersonalSkillDataExtractor
	{
		private readonly IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;

		public CreatePersonalSkillDataExtractorOLD(IScheduleResultDataExtractorProvider scheduleResultDataExtractorProvider)
		{
			_scheduleResultDataExtractorProvider = scheduleResultDataExtractorProvider;
		}
		
		public IScheduleResultDataExtractor Create(IScheduleMatrixPro scheduleMatrix, IOptimizationPreferences optimizationPreferences,
			ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			return _scheduleResultDataExtractorProvider.CreatePersonalSkillDataExtractor(scheduleMatrix,
				optimizationPreferences.Advanced, schedulingResultStateHolder);
		}
	}
	
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MinimumStaffing_75339)]
	public interface ICreatePersonalSkillDataExtractor
	{
		IScheduleResultDataExtractor Create(IScheduleMatrixPro scheduleMatrix, IOptimizationPreferences optimizationPreferences,
			ISchedulingResultStateHolder schedulingResultStateHolder);
	}
}