using System.Collections.ObjectModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for the objects that has a parent.
    /// </summary>
    /// <remarks>
    /// Usually for those entities where we only store a reference to the parent.
    /// Obviously that is not a classic parent-child connection, as the Children
    /// collection is missing.
    /// </remarks>
    public interface IParentChildEntity : IAggregateRoot
    {

        /// <summary>
        /// Gets or sets the parent as object.
        /// </summary>
        /// <value>The parent.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/5/2007
        /// </remarks>
        IParentChildEntity Parent { get; set; }

        /// <summary>
        /// Gets the current level of the hierarchy.
        /// </summary>
        /// <value>The level.</value>
        int Level { get; }

        /// <summary>
        /// Gets the child collection.
        /// </summary>
        /// <value>The child collection.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/12/2007
        /// </remarks>
        ReadOnlyCollection<IParentChildEntity> ChildCollection { get; }

        /// <summary>
        /// Adds the child.
        /// </summary>
        /// <param name="child">The child.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/12/2007
        /// </remarks>
        void AddChild(IParentChildEntity child);

        /// <summary>
        /// Removes the child.
        /// </summary>
        /// <param name="child">The child.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/12/2007
        /// </remarks>
        void RemoveChild(IParentChildEntity child);

    }
}