namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Create periods for the different needs from the input period.
    /// </summary>
    public interface IFullWeekOuterWeekPeriodCreator
    {
        /// <summary>
        /// Equals the Utc date of the schedule period.
        /// </summary>
        /// <returns></returns>
        DateOnlyPeriod EffectivePeriod();

        /// <summary>
        /// Extendeds the <see cref="EffectivePeriod"/> to whole week.
        /// </summary>
        /// <returns></returns>
        DateOnlyPeriod FullWeekPeriod();

        /// <summary>
        /// Adds a week before and after to the <see cref="FullWeekPeriod"/>
        /// </summary>
        /// <returns></returns>
        DateOnlyPeriod OuterWeekPeriod();

        /// <summary>
        /// Gets the person, the owner of the schedule period.
        /// </summary>
        /// <value>The person.</value>
        IPerson Person { get; }
        
    }
}