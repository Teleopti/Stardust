namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Creates an PersonAbsence
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2010-07-06
    /// </remarks>
    public interface ICreatePersonAbsence
    {
        /// <summary>
        /// Creates the PersonAbsence
        /// </summary>
        /// <param name="absence">The absence.</param>
        /// <param name="period">The period.</param>
        /// <param name="scenario">The scenario.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2010-07-06
        /// </remarks>
        IPersonAbsence Create(IAbsence absence, DateTimePeriod period, IScenario scenario, IPerson person);
    }
}