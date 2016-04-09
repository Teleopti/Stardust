using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Represents a list of <see cref="IScheduleDayPro"/> in the optimization process, plus some necessary properties and methods that are
    /// common in each item.
    /// </summary>
    public interface IScheduleMatrixPro
    {
	    bool IsDayLocked(DateOnly date);

        /// <summary>
        /// Gets or sets the state holder.
        /// </summary>
        /// <value>The state holder.</value>
        ISchedulingResultStateHolder SchedulingStateHolder { get;}

        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>The person.</value>
        IPerson Person { get; }

        /// <summary>
        /// Gets the contained schedule period days.
        /// </summary>
        /// <value>The schedule days.</value>
        ReadOnlyCollection<IScheduleDayPro> EffectivePeriodDays { get; }

        /// <summary>
        /// Gets the unlocked days.
        /// </summary>
        /// <value>The un locked days.</value>
        ReadOnlyCollection<IScheduleDayPro> UnlockedDays { get; }

        /// <summary>
        /// Gets the full weeks period days.
        /// </summary>
        /// <value>The outer period days.</value>
        ReadOnlyCollection<IScheduleDayPro> FullWeeksPeriodDays { get; }

        /// <summary>
        /// Gets the full weeks, extended by a week, period days.
        /// </summary>
        /// <value>The outer period days.</value>
        ReadOnlyCollection<IScheduleDayPro> OuterWeeksPeriodDays { get; }

        /// <summary>
        /// Gets the full week days plus the week before.
        /// </summary>
        /// <value>The full week days plus the week before.</value>
        ReadOnlyCollection<IScheduleDayPro> WeekBeforeOuterPeriodDays { get; }

        /// <summary>
        /// Gets the full week days plus the week after.
        /// </summary>
        /// <value>The full week days plus the week after.</value>
        ReadOnlyCollection<IScheduleDayPro> WeekAfterOuterPeriodDays { get; }

        /// <summary>
        /// Gets the full week plus the week before the full week period dictionary.
        /// </summary>
        /// <value>The full week plus the week before the full week period dictionary.</value>
        IDictionary<DateOnly, IScheduleDayPro> WeekBeforeOuterPeriodDictionary { get; }

        /// <summary>
        /// Gets the full week plus the week after the full week period dictionary.
        /// </summary>
        /// <value>The full week plus the week after the full week period dictionary.</value>
        IDictionary<DateOnly, IScheduleDayPro> WeekAfterOuterPeriodDictionary { get; }

        /// <summary>
        /// Gets the outer weeks period dictionary.
        /// </summary>
        /// <value>The outer weeks period dictionary.</value>
        IDictionary<DateOnly, IScheduleDayPro> OuterWeeksPeriodDictionary { get; }

        /// <summary>
        /// Gets the full weeks period dictionary.
        /// </summary>
        /// <value>The full weeks period dictionary.</value>
        IDictionary<DateOnly, IScheduleDayPro> FullWeeksPeriodDictionary { get; }

		/// <summary>
		/// Unlock a period of days.
		/// </summary>
		/// <param name="period">The period.</param>
		void UnlockPeriod(DateOnlyPeriod period);

        /// <summary>
        /// Locks the given dateonly period.
        /// </summary>
        /// <param name="period">The period.</param>
        void LockPeriod(DateOnlyPeriod period);

        /// <summary>
        /// Gets the schedule day by key.
        /// </summary>
        /// <param name="dateOnly">The date only.</param>
        /// <returns></returns>
        IScheduleDayPro GetScheduleDayByKey(DateOnly dateOnly);

        /// <summary>
        /// Gets the schedule period.
        /// </summary>
        /// <value>The schedule period.</value>
        IVirtualSchedulePeriod SchedulePeriod { get; }

        /// <summary>
        /// Gets the active schedule range.
        /// </summary>
        /// <value>The active schedule range.</value>
        IScheduleRange ActiveScheduleRange { get; }
    	
    }
}