namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Performs a complete set of day off moves.
    /// </summary>
    public interface IDayOffOptimizerContainer
    {
        /// <summary>
        /// Executes a day off move. Moves a set of days, mostly only one, and reschedule it.
        /// </summary>
        /// <returns>True if the move was successfull, false if not.</returns>
        bool Execute();

        /// <summary>
        /// Gets the container owner.
        /// </summary>
        /// <value>The container owner.</value>
        IPerson Owner { get; }

    }
}