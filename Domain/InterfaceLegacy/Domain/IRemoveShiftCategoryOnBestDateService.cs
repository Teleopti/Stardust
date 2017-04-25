using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Interface for RemoveShiftCategoryOnBestDateService
    /// </summary>
    public interface IRemoveShiftCategoryOnBestDateService
    {
		/// <summary>
		/// Excecutes this instance.
		/// </summary>
		/// <param name="shiftCategory">The shift category.</param>
		/// <param name="schedulingOptions">The scheduling options.</param>
		/// <returns></returns>
		IScheduleDayPro ExecuteOne(IShiftCategory shiftCategory, SchedulingOptions schedulingOptions);

		/// <summary>
		/// Executes the specified shift category.
		/// </summary>
		/// <param name="shiftCategory">The shift category.</param>
		/// <param name="period">The period.</param>
		/// <param name="schedulingOptions">The scheduling options.</param>
		/// <returns></returns>
		IScheduleDayPro ExecuteOne(IShiftCategory shiftCategory, DateOnlyPeriod period, SchedulingOptions schedulingOptions);

        /// <summary>
        /// Determines whether this day matches the shiftCategory.
        /// </summary>
        /// <param name="scheduleDayPro">The schedule day pro.</param>
        /// <param name="shiftCategory">The shift category.</param>
        /// <returns>
        /// </returns>
        bool IsThisDayCorrectShiftCategory(IScheduleDayPro scheduleDayPro, IShiftCategory shiftCategory);
    }
}