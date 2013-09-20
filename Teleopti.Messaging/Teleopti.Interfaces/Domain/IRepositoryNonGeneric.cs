namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for the non-generic repository base methods.
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// Adds the specified entity to repository.
        /// Will be persisted when PersistAll is called (or sooner).
        /// </summary>
        /// <param name="root">The entity.</param>
        void Add(IAggregateRoot root);

        /// <summary>
        /// Removes the specified entity from repository.
        /// Will be deleted when PersistAll is called (or sooner).
        /// </summary>
        /// <param name="root">The entity.</param>
        void Remove(IAggregateRoot root);
    }
}