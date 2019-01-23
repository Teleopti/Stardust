using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Represents a list of <see cref="IScheduleDayPro"/> in the optimization process, plus some necessary properties and methods that are
    /// common in each item.
    /// </summary>
    public interface IScheduleMatrixPro
    {
	    bool IsDayLocked(DateOnly date);

        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>The person.</value>
        IPerson Person { get; }

        /// <summary>
        /// Gets the contained schedule period days.
        /// </summary>
        /// <value>The schedule days.</value>
        IScheduleDayPro[] EffectivePeriodDays { get; }

        /// <summary>
        /// Gets the unlocked days.
        /// </summary>
        /// <value>The un locked days.</value>
        HashSet<IScheduleDayPro> UnlockedDays { get; }

        /// <summary>
        /// Gets the full weeks period days.
        /// </summary>
        /// <value>The outer period days.</value>
        IScheduleDayPro[] FullWeeksPeriodDays { get; }

        /// <summary>
        /// Gets the full weeks, extended by a week, period days.
        /// </summary>
        /// <value>The outer period days.</value>
        IScheduleDayPro[] OuterWeeksPeriodDays { get; }

        /// <summary>
        /// Gets the full week days plus the week before.
        /// </summary>
        /// <value>The full week days plus the week before.</value>
        IScheduleDayPro[] WeekBeforeOuterPeriodDays { get; }

        /// <summary>
        /// Gets the full week days plus the week after.
        /// </summary>
        /// <value>The full week days plus the week after.</value>
        IScheduleDayPro[] WeekAfterOuterPeriodDays { get; }

        /// <summary>
        /// Gets the outer weeks period dictionary.
        /// </summary>
        /// <value>The outer weeks period dictionary.</value>
        IDictionary<DateOnly, IScheduleDayPro> OuterWeeksPeriodDictionary { get; }

		/// <summary>
		/// Unlock a period of days.
		/// </summary>
		/// <param name="period">The period.</param>
		void UnlockPeriod(DateOnlyPeriod period);

        void LockDay(DateOnly date);

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

	    bool IsFullyScheduled();
    }
}