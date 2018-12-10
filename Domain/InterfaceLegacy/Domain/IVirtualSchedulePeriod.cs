using System;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// 
    /// </summary>
    public interface IVirtualSchedulePeriod
    {
        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        bool IsValid { get; }
        /// <summary>
        /// Gets the type of the period.
        /// </summary>
        /// <value>The type of the period.</value>
        SchedulePeriodType PeriodType { get; }
        /// <summary>
        /// Gets the number.
        /// </summary>
        /// <value>The number.</value>
        int Number { get; }
        /// <summary>
        /// Gets the average work time per day.
        /// </summary>
        /// <value>The average work time per day.</value>
        TimeSpan AverageWorkTimePerDay { get; }
        /// <summary>
        /// Periods the target.
        /// </summary>
        /// <returns></returns>
        TimeSpan PeriodTarget();
        /// <summary>
        /// Gets the workdays.
        /// </summary>
        /// <returns></returns>
        int Workdays();
        /// <summary>
        /// Gets the days off.
        /// </summary>
        /// <returns></returns>
        int DaysOff();
        /// <summary>
        /// Gets the date only period.
        /// </summary>
        /// <value>The date only period.</value>
        DateOnlyPeriod DateOnlyPeriod { get; }
        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <value>The person.</value>
        IPerson Person { get; }

        /// <summary>
        /// Shifts the category limitation collection.
        /// </summary>
        /// <returns></returns>
        ReadOnlyCollection<IShiftCategoryLimitation> ShiftCategoryLimitationCollection();

        ///// <summary>
        ///// Gets the person period.
        ///// </summary>
        ///// <value>The person period.</value>
        //IPersonPeriod PersonPeriod { get; }

        /// <summary>
        /// Gets the must have preference.
        /// </summary>
        /// <value>The must have preference.</value>
        int MustHavePreference { get; }

        /// <summary>
        /// Gets the min time schedule period.
        /// </summary>
        /// <value>The min time schedule period.</value>
        TimeSpan MinTimeSchedulePeriod { get; }


		/// <summary>
		/// Gets the contract.
		/// </summary>
		/// <value>The contract.</value>
    	IContract Contract { get; }

		/// <summary>
		/// Gets the contract schedule.
		/// </summary>
		/// <value>The contract schedule.</value>
    	IContractSchedule ContractSchedule { get; }

		/// <summary>
		/// Gets the part time percentage.
		/// </summary>
		/// <value>The part time percentage.</value>
    	IPartTimePercentage PartTimePercentage { get; }

        ///// <summary>
        ///// Gets the rule set bag.
        ///// </summary>
        ///// <value>The rule set bag.</value>
        //IRuleSetBag RuleSetBag { get; }

		/// <summary>
		/// Gets the balance in.
		/// </summary>
		/// <value>The balance in.</value>
		TimeSpan BalanceIn { get; }

		/// <summary>
		/// Gets the balance out.
		/// </summary>
		/// <value>The balance out.</value>
		TimeSpan BalanceOut { get; }

		/// <summary>
		/// Gets the extra.
		/// </summary>
		/// <value>The extra.</value>
		TimeSpan Extra { get; }

        /// <summary>
        /// Gets the seasonality.
        /// </summary>
        /// <value>The seasonality.</value>
        Percent Seasonality { get; }

        ///// <summary>
        ///// Gets the team.
        ///// </summary>
        ///// <value>The team.</value>
        //ITeam Team { get; }

        ///// <summary>
        ///// Gets the site.
        ///// </summary>
        ///// <value>The site.</value>
        //ISite Site { get; }

        /// <summary>
        /// Determines whether [is original period].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is original period]; otherwise, <c>false</c>.
        /// </returns>
        bool IsOriginalPeriod();
    }
}
