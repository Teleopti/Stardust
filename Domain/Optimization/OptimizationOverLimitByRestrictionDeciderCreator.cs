using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	 
	// Just a stupid wrapper to create a class so I don't need to change the constructors and remove all state in the 
	//RestrictionChecker, OptimizationOverLimitByRestrictionDecider and RestrictionOverLimitDecider
	public interface IOptimizationOverLimitByRestrictionDeciderCreator
	{
		IOptimizationOverLimitByRestrictionDecider GetDecider(DateOnly dateOnly, IScheduleMatrixPro scheduleMatrixPro,
																			 IOptimizationPreferences optimizationPreferences);
	}

	public class OptimizationOverLimitByRestrictionDeciderCreator : IOptimizationOverLimitByRestrictionDeciderCreator
	{
		public IOptimizationOverLimitByRestrictionDecider GetDecider(DateOnly dateOnly, IScheduleMatrixPro scheduleMatrixPro,
			IOptimizationPreferences optimizationPreferences)
		{
			var restrictionChecker = new RestrictionChecker(scheduleMatrixPro.GetScheduleDayByKey(dateOnly).DaySchedulePart());
			//the originalstatecontainers is needed when checkin MaxMovedDays
			return new OptimizationOverLimitByRestrictionDecider(scheduleMatrixPro, restrictionChecker, optimizationPreferences, null);
		}
	}
}