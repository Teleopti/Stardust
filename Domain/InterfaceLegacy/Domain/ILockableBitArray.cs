using System;
using System.Collections;
using System.Collections.Generic;
using Teleopti.Ccc.Secrets.DayOffPlanning;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Internally two bittarays of same length, one handling value and the other handling locks
    /// </summary>
    public interface ILockableBitArray : IDayOffBitArray, ICloneable
    {
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
        /// Locks the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        void Lock(int index, bool value);

        /// <summary>
        /// Gets the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        bool Get(int index);

        /// <summary>
        /// Returns a bitarray that will always represent outerweekperiod.
        /// </summary>
        /// <returns></returns>
        BitArray ToLongBitArray();

	    bool HasSameDayOffs(ILockableBitArray otherLockableBitArray);
    }
}