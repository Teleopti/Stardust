using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
		///<summary>
    /// IPersonAbsenceAccount
    ///</summary>
    public interface IPersonAbsenceAccount 
        : IAggregateRoot, IVersioned, IOriginator<IPersonAbsenceAccount>, ICloneableEntity<IPersonAbsenceAccount>
    {
        ///<summary>
        ///</summary>
        IEnumerable<IAccount> AccountCollection();

        ///<summary>
        ///</summary>
        IPerson Person { get; }

        ///<summary>
        ///</summary>
        IAbsence Absence { get; }

        ///<summary>
        ///</summary>
        ///<param name="account"></param>
        void Add(IAccount account);

        ///<summary>
        ///</summary>
        ///<param name="account"></param>
        void Remove(IAccount account);

        /// <summary>
        /// Finds the specified date time.
        /// </summary>
        /// <param name="dateOnly">The date only.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2010-04-28
        /// </remarks>
        IAccount Find(DateOnly dateOnly);

        /// <summary>
        /// Finds the specified date time period.
        /// </summary>
        /// <param name="dateOnlyPeriod">The date only period.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2010-04-30
        /// </remarks>
        IEnumerable<IAccount> Find(DateOnlyPeriod dateOnlyPeriod);
    }
}
