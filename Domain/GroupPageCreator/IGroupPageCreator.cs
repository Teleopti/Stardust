using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.GroupPageCreator
{
    /// <summary>
    /// Defines the functionality of creator of GroupPage.
    /// </summary>
    public interface IGroupPageCreator<T>
    {
        /// <summary>
        /// Creates the group page.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-06-25
        /// </remarks>
        IGroupPage CreateGroupPage(IEnumerable<T> entityCollection, IGroupPageOptions groupPageOptions);
    }
}