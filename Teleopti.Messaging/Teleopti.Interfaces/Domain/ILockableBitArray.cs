using System;
using System.Collections;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Internally two bittarays of same length, one handling value and the other handling locks
    /// </summary>
    public interface ILockableBitArray : ICloneable
    {
        /// <summary>
        /// Gets the days off bit array.
        /// </summary>
        /// <value>The days off bit array.</value>
        BitArray DaysOffBitArray { get; }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        int Count { get; }

        /// <summary>
        /// Gets or sets the area corresponding to the schedule period.
        /// </summary>
        /// <value>The period area.</value>
        MinMax<int> PeriodArea { get; set; }

        /// <summary>
        /// Gets the unlocked indexes.
        /// </summary>
        /// <value>The unlocked indexes.</value>
        IList<int> UnlockedIndexes { get; }

        /// <summary>
        /// Gets the index of the terminal date.
        /// </summary>
        /// <value>The index of the terminal date.</value>
        int? TerminalDateIndex { get; }

        /// <summary>
        /// Sets the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        void Set(int index, bool value);

        /// <summary>
        /// Sets all.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        void SetAll(bool value);

        /// <summary>
        /// Gets the <see cref="System.Boolean"/> at the specified index.
        /// </summary>
        /// <value></value>
        bool this[int index] { get; }

        /// <summary>
        /// Locks the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        void Lock(int index, bool value);

        /// <summary>
        /// Finds a random index of that is not locked.
        /// </summary>
        /// <returns></returns>
        int FindRandomUnlockedIndex();

        /// <summary>
        /// Gets the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        bool Get(int index);

        /// <summary>
        /// Determines whether the specified index is locked.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="considerLock">if set to <c>true</c> [consider lock].</param>
        /// <returns>
        /// 	<c>true</c> if the specified index is locked; otherwise, <c>false</c>.
        /// </returns>
        bool IsLocked(int index, bool considerLock);

        /// <summary>
        /// Returns a bitarray that will always represent outerweekperiod.
        /// </summary>
        /// <returns></returns>
        BitArray ToLongBitArray();
    }
}