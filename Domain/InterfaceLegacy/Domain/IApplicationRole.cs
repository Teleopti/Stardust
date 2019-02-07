using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// An application role, eg Person or TeamLeader
    /// </summary>
    public interface IApplicationRole : IAggregateRoot, IFilterOnBusinessUnit
    {
        /// <summary>
        /// Sets the business unit.
        /// </summary>
        /// <value>The business unit.</value>
        void SetBusinessUnit(IBusinessUnit businessUnit);

        /// <summary>
        /// Gets the application function collection.
        /// </summary>
        /// <value>The application function collection.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-12
        /// </remarks>
        ICollection<IApplicationFunction> ApplicationFunctionCollection { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the role is built in in the raptor system.
        /// </summary>
        /// <value><c>true</c> if built in; otherwise, <c>false</c>.</value>
        bool BuiltIn { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        string DescriptionText { get; set; }

        /// <summary>
        /// Adds the application function if does not exist in the list.
        /// </summary>
        /// <param name="applicationFunction">The function.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-12
        /// </remarks>
        void AddApplicationFunction(IApplicationFunction applicationFunction);

        /// <summary>
        /// Removes the application function if exist.
        /// </summary>
        /// <param name="applicationFunction">The function.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-12
        /// </remarks>
        bool RemoveApplicationFunction(IApplicationFunction applicationFunction);

        /// <summary>
        /// Gets or sets the available data that belongs to the application role.
        /// </summary>
        /// <value>The available data.</value>
        /// <remarks>
        /// Note: This property is filled up when the AvailableDate collection is loaded.
        /// </remarks>
        IAvailableData AvailableData { get; set; }
    }
}
