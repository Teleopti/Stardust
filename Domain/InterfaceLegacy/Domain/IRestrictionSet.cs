namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for restriction sets
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-11-07
    /// </remarks>
    public interface IRestrictionSet
    {
        /// <summary>
        /// Checks the entity.
        /// </summary>
        /// <param name="entityToCheck">The entity to check.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-07
        /// </remarks>
        void CheckEntity(object entityToCheck);
    }

    /// <summary>
    /// Generic interface for restriction sets
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-11-07
    /// </remarks>
    public interface IRestrictionSet<T> where T : IAggregateRoot
    {
        /// <summary>
        /// Checks the entity.
        /// </summary>
        /// <param name="entityToCheck">The entity to check.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-07
        /// </remarks>
        void CheckEntity(T entityToCheck);
    }
}