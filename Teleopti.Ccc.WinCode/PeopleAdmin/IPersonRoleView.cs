using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.WinCode.PeopleAdmin
{
    /// <summary>
    /// Interface for person role to implement MVC.This will include mapping fuctionality between 
    /// Model and View of person role.
    /// </summary>
    /// <remarks>
    /// Created by: Dinesh Ranasinghe
    /// Created date: 2008-02-14
    /// </remarks>
    public interface IPersonRoleView
    {

        /// <summary>
        /// Loads the selected persons.
        /// </summary>
        /// <param name="personCollection">The person collection.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-02-15
        /// </remarks>
        int LoadSelectedPersons(IList<Person> personCollection);
    }
}