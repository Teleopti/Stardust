namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Used to calculate fairness of the schedule
    /// </summary>
    public interface IScheduleFairnessCalculator
    {
        /// <summary>
        /// Returns the fairness value for this person.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        double PersonFairness(IPerson person);

        /// <summary>
        /// Returns the total fairness for this schedule.
        /// </summary>
        /// <returns></returns>
        double ScheduleFairness();
    }
}