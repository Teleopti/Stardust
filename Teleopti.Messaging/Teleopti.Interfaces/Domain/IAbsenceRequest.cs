namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Request for absence
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-10-10
    /// </remarks>
    public interface IAbsenceRequest : IRequest
    {
        /// <summary>
        /// Gets the absence.
        /// </summary>
        /// <value>The absence.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-05
        /// </remarks>
        IAbsence Absence { get; }

	    bool IsWaitlisted();
    }
}