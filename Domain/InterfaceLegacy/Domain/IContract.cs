using System;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Represents a general contract between employer and employees. Contains the
    /// conditions and directives of the work. 
    /// </summary>
    public interface IContract : IAggregateRoot,
									IChangeInfo, IFilterOnBusinessUnit
    {
        /// <summary>
        /// Name of Contract
        /// </summary>
        Description Description { get; set; }

        /// <summary>
        /// Type of employees at contract
        /// </summary>
        EmploymentType EmploymentType { get; set; }

        /// <summary>
        /// Work time directive informaton of contract
        /// </summary>
        WorkTimeDirective WorkTimeDirective { get; set; }

        /// <summary>
        /// Work time informaton of contract
        /// </summary>
        WorkTime WorkTime { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is choosable.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is choosable; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-09-09
        /// </remarks>
        bool IsChoosable { get; }

        /// <summary>
        /// Gets or sets the positive period work time tolerance.
        /// </summary>
        /// <value>The positive period work time tolerance.</value>
        TimeSpan PositivePeriodWorkTimeTolerance { get; set; }

        /// <summary>
        /// Gets or sets the negative period work time tolerance.
        /// </summary>
        /// <value>The negative period work time tolerance (not negated).</value>
        TimeSpan NegativePeriodWorkTimeTolerance { get; set; }

        /// <summary>
        /// Get or sets the minimum time for hourly employees on a schedule period
        /// </summary>
        TimeSpan MinTimeSchedulePeriod { get; set; }

        /// <summary>
        /// Gets the multiplicator definition set collection.
        /// </summary>
        /// <value>The multiplicator definition set collection.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-02-04
        /// </remarks>
        ReadOnlyCollection<IMultiplicatorDefinitionSet> MultiplicatorDefinitionSetCollection { get; }

        /// <summary>
        /// Gets or sets the planning time bank max.
        /// </summary>
        /// <value>
        /// The planning time bank max.
        /// </value>
        TimeSpan PlanningTimeBankMax { get; set; }
        /// <summary>
        /// Gets or sets the planning time bank min.
        /// </summary>
        /// <value>
        /// The planning time bank min.
        /// </value>
        TimeSpan PlanningTimeBankMin { get; set; }

        /// <summary>
        /// Adds the multiplicator definition set to the collection.
        /// </summary>
        /// <param name="definitionSet">The definition set.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-02-04
        /// </remarks>
        void AddMultiplicatorDefinitionSetCollection(IMultiplicatorDefinitionSet definitionSet);

        /// <summary>
        /// Removes the multiplicator definition set from the collection.
        /// </summary>
        /// <param name="definitionSet">The definition set.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-02-04
        /// </remarks>
        void RemoveMultiplicatorDefinitionSetCollection(IMultiplicatorDefinitionSet definitionSet);

        /// <summary>
        /// Gets or sets a value indicating whether to adjust time bank with seasonality.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [adjust time bank with seasonality]; otherwise, <c>false</c>.
        /// </value>
        bool AdjustTimeBankWithSeasonality { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to adjust time bank with part time percentage.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [adjust time bank with seasonality]; otherwise, <c>false</c>.
        /// </value>
        bool AdjustTimeBankWithPartTimePercentage { get; set; }

        /// <summary>
        /// Gets or sets the positive day off tolerance.
        /// </summary>
        /// <value>
        /// The positive day off tolerance.
        /// </value>
        int PositiveDayOffTolerance { get; set; }

        /// <summary>
        /// Gets or sets the negative day off tolerance.
        /// </summary>
        /// <value>
        /// The negative day off tolerance.
        /// </value>
        int NegativeDayOffTolerance { get; set; }
        
        /// <summary>
        /// Gets or sets the from schedule period
        /// </summary>
        WorkTimeSource WorkTimeSource { get; set; }
    }
}
