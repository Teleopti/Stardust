using System;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Class to hold scheduling periods
    /// </summary>
    /// <remarks>
    /// Created by: cs 
    /// Created date: 2008-03-10
    /// </remarks>
    public interface ISchedulePeriod : IAggregateEntity, 
                                        ICloneable
    {
        /// <summary>
        /// gets date from
        /// </summary>
        /// <remarks>
        /// Created by: cs 
        /// Created date: 2008-03-10
        /// </remarks>
        DateOnly DateFrom { get; set; }

        /// <summary>
        /// gets type
        /// </summary>
        /// <remarks>
        /// Created by: cs 
        /// Created date: 2008-03-10
        /// </remarks>
        SchedulePeriodType PeriodType { get; set; }

        /// <summary>
        /// gets number
        /// </summary>
        /// <remarks>
        /// Created by: cs 
        /// Created date: 2008-03-10
        /// </remarks>
        int Number { get; set; }

        /// <summary>
        /// Gets the average work time per day.
        /// </summary>
        /// <remarks>
        /// Created by: cs 
        /// Created date: 2008-03-10
        /// </remarks>
        TimeSpan AverageWorkTimePerDay { get; }

		/// <summary>
		/// Gets or sets the average work time per day for display.
		/// </summary>
		/// <value>The average work time per day for display.</value>
		/// <remarks>
		/// Created by: cs 
		/// Created date: 2008-03-10
		/// </remarks>
		TimeSpan AverageWorkTimePerDayOverride { get; set; }

		/// <summary>
		/// Gets a value indicating whether the period is overriden.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the period value is overriden; otherwise, <c>false</c>.
		/// </value>
		/// <remarks>
		/// Created by: tamasb
		/// Created date: 2012-06-15
		/// </remarks>
		bool IsPeriodTimeOverride { get; }

		/// <summary>
		/// Gets or sets the period time.
		/// </summary>
		/// <value>The period time.</value>
		/// <remarks>
		/// Created by: tamasb
		/// Created date: 2012-06-15
		/// </remarks>
		TimeSpan? PeriodTime
		{ get; set; }

        /// <summary>
        /// Gets the real schedule period with a real start and an end date. If the parameter is not part of the period, 
        /// will return null.
        /// </summary>
        /// <param name="dateValue"></param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: cs 
        /// Created date: 2008-03-10
        /// </remarks>
        DateOnlyPeriod? GetSchedulePeriod(DateOnly dateValue);

        /// <summary>
        /// Gets or sets the days off.
        /// </summary>
        /// <value>The days off.</value>
        /// remove this suppress when GetDaysOff is removed
        int? DaysOff {get ; set ; }
        
        /// <summary>
        /// Gets the contract schedule days off during this schedule period.
        /// </summary>
        /// <param name="dateFrom">The date from.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-11-03
        /// </remarks>
        int GetDaysOff(DateOnly dateFrom);

        /// <summary>
        /// SetDaysOff
        /// </summary>
        /// <param name="value"></param>
        void SetDaysOff(int value);

        /// <summary>
        /// Shift category limitation collection.
        /// </summary>
        /// <returns></returns>
        ReadOnlyCollection<IShiftCategoryLimitation> ShiftCategoryLimitationCollection();

        /// <summary>
        /// Adds the shift category limitation.
        /// </summary>
		/// <param name="shiftCategoryLimitationToAdd">The shift category limitation.</param>
		void AddShiftCategoryLimitation(IShiftCategoryLimitation shiftCategoryLimitationToAdd);

        /// <summary>
        /// Removes the shift category limitation.
        /// </summary>
        /// <param name="shiftCategory">The shift category.</param>
        void RemoveShiftCategoryLimitation(IShiftCategory shiftCategory);

        /// <summary>
        /// Gets or sets the must have preference.
        /// </summary>
        /// <value>The must have preference.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2010-02-08
        /// </remarks>
        int MustHavePreference { set; get; }

        /// <summary>
        /// Gets a value indicating whether this instance is average work time per day override.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is average work time per day override; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-10
        /// </remarks>
        bool IsAverageWorkTimePerDayOverride { get; }


        /// <summary>
        /// Gets or sets the balance in.
        /// </summary>
        /// <value>The balance in.</value>
        TimeSpan BalanceIn { get; set; }

        /// <summary>
        /// Gets or sets the balance out.
        /// </summary>
        /// <value>The balance out.</value>
        TimeSpan BalanceOut { get; set; }

        /// <summary>
        /// Gets or sets the extra.
        /// </summary>
        /// <value>The extra.</value>
        TimeSpan Extra { get; set; }

        /// <summary>
        /// Gets or sets the seasonality.
        /// </summary>
        /// <value>The seasonality.</value>
        Percent Seasonality { get; set; } 

		/// <summary>
		/// Returns the real date to
		/// </summary>
		/// <returns></returns>
    	DateOnly RealDateTo();
    }
}
