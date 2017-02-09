using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class SetMainShiftOptimizeActivitySpecificationForTeamBlock
	{
		private readonly IUserTimeZone _userTimeZone;
		private readonly OptimizerActivitiesPreferencesFactory _optimizerActivitiesPreferencesFactory;

		public SetMainShiftOptimizeActivitySpecificationForTeamBlock(IUserTimeZone userTimeZone, OptimizerActivitiesPreferencesFactory optimizerActivitiesPreferencesFactory)
		{
			_userTimeZone = userTimeZone;
			_optimizerActivitiesPreferencesFactory = optimizerActivitiesPreferencesFactory;
		}

		public void Execute(IOptimizationPreferences optimizationPreferences, ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions)
		{
			var userTimeZone = _userTimeZone.TimeZone();
			var optimizerActivitiesPreferences = _optimizerActivitiesPreferencesFactory.Create(optimizationPreferences);
			if (optimizerActivitiesPreferences != null)
			{
				ISpecification<IEditableShift> specification = new All<IEditableShift>();
				foreach (var unLockedDate in teamBlockInfo.BlockInfo.UnLockedDates())
				{
					foreach (var scheduleMatrixPro in teamBlockInfo.MatrixesForGroupAndBlock())
					{
						var editableShift = scheduleMatrixPro.GetScheduleDayByKey(unLockedDate).DaySchedulePart().GetEditorShift();
						specification =
							specification.And(new MainShiftOptimizeActivitiesSpecification(optimizerActivitiesPreferences, editableShift,
								unLockedDate, userTimeZone));
					}
				}
				schedulingOptions.MainShiftOptimizeActivitySpecification = specification;
			}
		}
	}
}