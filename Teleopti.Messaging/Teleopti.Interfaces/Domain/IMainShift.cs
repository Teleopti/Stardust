namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Contains the collection of working activities for a person's work assignment(<see cref="IPersonAssignment"/>).
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-08-07
    /// </remarks>
	public interface IMainShift : IShift, IAggregateEntity
    {
        /// <summary>
        /// Gets or the shift category
        /// </summary>
        IShiftCategory ShiftCategory { get; set; }
    }
}