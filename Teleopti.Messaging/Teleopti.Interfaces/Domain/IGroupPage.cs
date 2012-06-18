using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.Domain
{
    ///<summary>
    ///</summary>
    public interface IGroupPage : IAggregateRoot
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-06-23
        /// </remarks>
        Description Description { get; set; }

        ///<summary>
        /// Get or sets the resource key that is used to localize the property <see cref="Description"/>.
        ///</summary>
        string DescriptionKey { get; set; }

        /// <summary>
        /// Gets the group collection.
        /// </summary>
        /// <value>The group collection.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-06-23
        /// </remarks>
        ReadOnlyCollection<IRootPersonGroup> RootGroupCollection { get; }

        ///<summary>
        /// Gets or sets the root node name, defaults to the name of the description
        ///</summary>
        string RootNodeName { get; set; }

        /// <summary>
        /// Adds the group unit.
        /// </summary>
        /// <param name="group">The top group unit.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-06-25
        /// 
        /// Modified by: kosalanp
        /// Modified date: 26/06/2008
        /// </remarks>
        void AddRootPersonGroup(IRootPersonGroup group);

        /// <summary>
        /// Removes the group unit.
        /// </summary>
        /// <param name="group">The top group unit.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-06-25
        /// 
        /// Modified by: kosalanp
        /// Modified date: 26/06/2008
        /// </remarks>
        void RemoveRootPersonGroup(IRootPersonGroup group);

        /// <summary>
        /// Gets the key used to save a selected GroupPage.
        /// </summary>
        string Key { get; }

		/// <summary>
		/// Returns true if the group page is user defined.
		/// </summary>
    	bool IsUserDefined();

		/// <summary>
		/// Used when when fetching user defined in scheduling
		/// </summary>
		string IdOrDescriptionKey { get; }
    }
}