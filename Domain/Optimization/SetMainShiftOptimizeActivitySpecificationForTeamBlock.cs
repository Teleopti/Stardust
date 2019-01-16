using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class SetMainShiftOptimizeActivitySpecificationForTeamBlock
	{
		private readonly OptimizerActivitiesPreferencesFactory _optimizerActivitiesPreferencesFactory;
		private readonly CorrectAlteredBetween _correctAlteredBetween;

		public SetMainShiftOptimizeActivitySpecificationForTeamBlock(
													OptimizerActivitiesPreferencesFactory optimizerActivitiesPreferencesFactory,
													CorrectAlteredBetween correctAlteredBetween)
		{
			_optimizerActivitiesPreferencesFactory = optimizerActivitiesPreferencesFactory;
			_correctAlteredBetween = correctAlteredBetween;
		}

		public void Execute(IOptimizationPreferences optimizationPreferences, ITeamBlockInfo teamBlockInfo, SchedulingOptions schedulingOptions)
		{
			var optimizerActivitiesPreferences = _optimizerActivitiesPreferencesFactory.Create(optimizationPreferences);
			if (optimizerActivitiesPreferences != null)
			{
				ISpecification<IEditableShift> specification = new All<IEditableShift>();
				foreach (var unLockedDate in teamBlockInfo.BlockInfo.UnLockedDates())
				{
					var unlockedMembers = teamBlockInfo.TeamInfo.UnLockedMembers(unLockedDate).ToHashSet();

					foreach (var scheduleMatrixPro in teamBlockInfo.MatrixesForGroupAndBlock())
					{
						if(!unlockedMembers.Contains(scheduleMatrixPro.Person))
							continue;

						var editableShift = scheduleMatrixPro.GetScheduleDayByKey(unLockedDate)?.DaySchedulePart().GetEditorShift();
						if(editableShift != null)
							specification = specification.And(new MainShiftOptimizeActivitiesSpecification(_correctAlteredBetween, optimizerActivitiesPreferences, editableShift, unLockedDate));
					}
				}
				schedulingOptions.MainShiftOptimizeActivitySpecification = specification;
			}
		}
	}
}