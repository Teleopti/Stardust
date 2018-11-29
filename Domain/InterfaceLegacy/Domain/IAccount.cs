using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Baseinterface for PersonAccount
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2008-08-21
    /// </remarks>
    public interface IAccount : IAggregateEntity, ITraceable, ICloneableEntity<IAccount>
    {
        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>The start date.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-08-21
        /// </remarks>
        DateOnly StartDate { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is exceeded.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is exceeded; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-03-02
        /// </remarks>
        bool IsExceeded { get; }

        ///<summary>
        ///</summary>
        TimeSpan BalanceOut { get; set; }

        /// <summary>
        /// Gets or sets the extra.
        /// </summary>
        /// <value>The extra.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-06-26
        /// </remarks>
        TimeSpan Extra { get; set; }
        
        /// <summary>
        /// Gets or sets the Accrued.
        /// </summary>
        /// <value>The Accrued.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-06-26
        /// </remarks>
        TimeSpan Accrued { get; set; }

        /// <summary>
        /// Gets or sets the BalanceIn.
        /// </summary>
        /// <value>The BalanceIn.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-06-26
        /// </remarks>
        TimeSpan BalanceIn { get; set; }

        /// <summary>
        /// Gets the Remaining.
        /// </summary>
        /// <value>The remaining time.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 2011-01-18
        /// </remarks>
        TimeSpan Remaining { get; }

        /// <summary>
        /// Gets or sets the LatestCalculatedBalance.
        /// </summary>
        /// <value>The LatestCalculatedBalance.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-06-26
        /// </remarks>
        TimeSpan LatestCalculatedBalance { get; }

        ///<summary>
        ///</summary>
        IPersonAbsenceAccount Owner { get; }

        /// <summary>
        /// Calculates the used by reading from the repository and doing a projection
        /// </summary>
        /// <param name="storage">The repository.</param>
        /// <param name="scenario">The scenario to look for.</param>
        /// <remarks>Henrik 090226</remarks>
        void CalculateUsed(IScheduleStorage storage, IScenario scenario);

        /// <summary>
        /// Calculates the period based on the next PersonAccounts StartDateTime
        /// </summary>
        /// <returns>
        /// Returns max endtime if its the last PersonAccount of this Absence
        /// </returns>
        DateOnlyPeriod Period();

     }
}
