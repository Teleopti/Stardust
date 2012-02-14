using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// 
    /// </summary>
	public interface ISchedulingObjectContainer
	{
		/// <summary>
		/// Gets the shift categories.
		/// </summary>
		/// <value>The shift categories.</value>
		IList<IShiftCategory> ShiftCategories { get; }

		/// <summary>
		/// Gets or sets the person skill periods data holder manager.
		/// </summary>
		/// <value>The person skill periods data holder manager.</value>
		IPersonSkillPeriodsDataHolderManager PersonSkillPeriodsDataHolderManager { get; set; }
		/// <summary>
		/// Gets or sets the shift projection cache manager.
		/// </summary>
		/// <value>The shift projection cache manager.</value>
		IShiftProjectionCacheManager ShiftProjectionCacheManager { get; set; }

		/// <summary>
		/// Gets or sets the options.
		/// </summary>
		/// <value>The options.</value>
		ISchedulingOptions Options { get; set; }


		/// <summary>
		/// Gets or sets the shift projection cashe filter.
		/// </summary>
		/// <value>The shift projection cashe filter.</value>
		IShiftProjectionCacheFilter ShiftProjectionCacheFilter { get; set; }

		/// <summary>
		/// Gets or sets the work shift calculator.
		/// </summary>
		/// <value>The work shift calculator.</value>
		IWorkShiftCalculator WorkShiftCalculator { get; set; }

		/// <summary>
		/// Gets or sets the fairness value calculator.
		/// </summary>
		/// <value>The fairness value calculator.</value>
		IFairnessValueCalculator FairnessValueCalculator { get; set; }

		/// <summary>
		/// Gets or sets the pre scheduling status checker.
		/// </summary>
		/// <value>The pre scheduling status checker.</value>
		IPreSchedulingStatusChecker PreSchedulingStatusChecker { get; set; }
	}
}
