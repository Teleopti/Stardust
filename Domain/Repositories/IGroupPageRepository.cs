using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IGroupPageRepository : IRepository<IGroupPage>
    {
        /// <summary>
        /// Loads the name of all group page by sorted by.
        /// </summary>
        /// <returns></returns>
        IList<IGroupPage> LoadAllGroupPageBySortedByDescription();

		/// <summary>
		/// Loads all group page when person collection is reassociated.
		/// </summary>
		/// <returns></returns>
    	IList<IGroupPage> LoadAllGroupPageWhenPersonCollectionReAssociated();

	    IList<IGroupPage> LoadGroupPagesByIds(IEnumerable<Guid> groupPageIds);

	    IList<IGroupPage> GetGroupPagesForPerson(Guid personId);

    }
}
