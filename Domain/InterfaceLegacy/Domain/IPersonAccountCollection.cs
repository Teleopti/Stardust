using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    ///<summary>
    /// All accounts for one person
    ///</summary>
    public interface IPersonAccountCollection : IEnumerable<IPersonAbsenceAccount>
    {
        /// <summary>
        /// Finds the specified absence.
        /// </summary>
        /// <param name="absence">The absence.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2010-04-29
        /// </remarks>
        IPersonAbsenceAccount Find(IAbsence absence);

        /// <summary>
        /// Finds the specified absence.
        /// </summary>
        /// <param name="absence">The absence.</param>
        /// <param name="dateOnly">The date only.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2010-04-29
        /// </remarks>
        IAccount Find(IAbsence absence, DateOnly dateOnly);

        /// <summary>
        /// Finds the specified date time.
        /// </summary>
        /// <param name="dateOnly">The date only.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2010-04-29
        /// </remarks>
        IEnumerable<IAccount> Find(DateOnly dateOnly);

        /// <summary>
        /// Adds the specified person absence account.
        /// </summary>
        /// <param name="personAbsenceAccount">The person absence account.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2010-04-28
        /// </remarks>
        void Add(IPersonAbsenceAccount personAbsenceAccount);

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <value>The person.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2010-04-29
        /// </remarks>
        IPerson Person { get; }

        /// <summary>
        /// Alls the person accounts.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2010-04-29
        /// </remarks>
        IEnumerable<IAccount> AllPersonAccounts();

        /// <summary>
        /// Returns a read only collection of the accounts
        /// </summary>
        /// <returns></returns>
        ReadOnlyCollection<IPersonAbsenceAccount> PersonAbsenceAccounts();

        /// <summary>
        /// Adds the specified absence.
        /// </summary>
        /// <param name="absence">The absence.</param>
        /// <param name="account">The account.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2010-04-29
        /// </remarks>
        void Add(IAbsence absence, IAccount account);

        /// <summary>
        /// Removes the specified account.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2010-04-29
        /// </remarks>
        void Remove(IAccount account);
    }
}
