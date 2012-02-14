namespace Teleopti.Interfaces.Domain
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
        /// <returns></returns>
        IScheduleDayPro ExecuteOne(IShiftCategory shiftCategory);

        /// <summary>
        /// Executes the specified shift category.
        /// </summary>
        /// <param name="shiftCategory">The shift category.</param>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        IScheduleDayPro ExecuteOne(IShiftCategory shiftCategory, DateOnlyPeriod period);

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