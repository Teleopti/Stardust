using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for all entities except aggregate roots in Domain.
    /// Always belong to a aggregate root one or more step above in tree.
    /// </summary>
    public interface IAggregateEntity : IEntity
    {
        /// <summary>
        /// Gets the entity parent of this entity.
        /// </summary>
        /// <value>The parent.</value>
        IEntity Parent { get; }

        /// <summary>
        /// Gets the root of this entity's aggregate.
        /// </summary>
        IAggregateRoot Root();

        /// <summary>
        /// Sets the parent.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-27
        /// </remarks>
        void SetParent(IEntity parent);
    }
}