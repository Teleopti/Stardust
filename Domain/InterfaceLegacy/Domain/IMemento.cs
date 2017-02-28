namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// A historical snapshot of an aggregate
    /// </summary>
    public interface IMemento
    {
        /// <summary>
        /// Restores the previous state of target.
        /// </summary>
        /// <returns>
        /// Returns the newly restored object.
        /// Needed to get Redo work properly
        /// </returns>
        IMemento Restore();
    }
}