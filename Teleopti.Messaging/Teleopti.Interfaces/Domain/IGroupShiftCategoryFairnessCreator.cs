namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for Group Shift Category Fairness Creator
    /// </summary>
    public interface IGroupShiftCategoryFairnessCreator
    {
        /// <summary>
        /// Calculates the group shift category fairness.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="dateOnly">The date only.</param>
        /// <returns></returns>
        IShiftCategoryFairness CalculateGroupShiftCategoryFairness(IPerson person, DateOnly dateOnly);
    }
}