using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness
{
	public interface IShiftCategoryFairnessReScheduler
	{
		bool Execute(IList<IPerson> persons, DateOnly dateOnly, IList<IScheduleMatrixPro> matrixes);
	}
	public class ShiftCategoryFairnessReScheduler : IShiftCategoryFairnessReScheduler
	{
		private readonly IOptimizationPreferences _optimizationPreferences;
		private readonly IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
		private readonly IGroupPersonConsistentChecker _groupPersonConsistentChecker;

		public ShiftCategoryFairnessReScheduler(IOptimizationPreferences optimizationPreferences, 
			IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization,
			IGroupPersonConsistentChecker groupPersonConsistentChecker)
		{
			_optimizationPreferences = optimizationPreferences;
			_groupPersonBuilderForOptimization = groupPersonBuilderForOptimization;
			_groupPersonConsistentChecker = groupPersonConsistentChecker;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool Execute(IList<IPerson> persons, DateOnly dateOnly, IList<IScheduleMatrixPro> matrixes )
		{
			if (persons.Count == 0) return true;
			var schedulingOptionsSynchronizer = new SchedulingOptionsCreator();
			var schedulingOptions = schedulingOptionsSynchronizer.CreateSchedulingOptions(_optimizationPreferences);

			var groupPersonToRun = _groupPersonBuilderForOptimization.BuildGroupPerson(persons[0], dateOnly);
			foreach (var person in persons)
			{
				if (!_groupPersonConsistentChecker.AllPersonsHasSameOrNoneScheduled(groupPersonToRun, dateOnly, schedulingOptions))
					return false;
			}
			return true;
		}
	}
}