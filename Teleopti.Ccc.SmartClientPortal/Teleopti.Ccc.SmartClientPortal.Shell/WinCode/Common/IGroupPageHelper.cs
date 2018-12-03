using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
    public interface IGroupPageHelper : IDisposable, IGroupPageDataProvider
    {
		IEnumerable<IPerson> PersonCollection { get; }
		
        /// <summary>
        /// Gets or sets the current group page.
        /// </summary>
        /// <value>The current group page.</value>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-07-04
        /// </remarks>
        IGroupPage CurrentGroupPage { get; set; }

        /// <summary>
        /// Loads all persons.
        /// </summary>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-06-26
        /// </remarks>
        void LoadAll();
		
    	/// <summary>
    	/// Removes the group page.
    	/// </summary>
    	/// <param name="groupPage">The group page.</param>
    	/// <remarks>
    	/// Created by: Muhamad Risath
    	/// Created date: 2008-07-10
    	/// </remarks>
    	IEnumerable<IRootChangeInfo> RemoveGroupPage(IGroupPage groupPage);

        void UpdateCurrent();

    	/// <summary>
    	/// Adds the or update group page.
    	/// </summary>
    	/// <param name="groupPage">The group page.</param>
    	IEnumerable<IRootChangeInfo> AddOrUpdateGroupPage(IGroupPage groupPage);

        void SetSelectedPeriod(DateOnlyPeriod dateOnlyPeriod);
        void RemoveGroupPageById(Guid id);
        void SetCurrentGroupPageById(Guid groupPageId);
        void RenameGroupPage(Guid groupPageId, string newName);
    }
}