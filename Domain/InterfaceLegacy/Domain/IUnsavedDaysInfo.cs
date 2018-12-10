using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    ///<summary>
    /// Unsaved days information
    ///</summary>
    public interface IUnsavedDaysInfo : IEquatable<IUnsavedDaysInfo>
    {
        ///<summary>
        /// Add an unsaved day.
        ///</summary>
        ///<param name="unsavedDayInfo"></param>
        void Add(IUnsavedDayInfo unsavedDayInfo);
        ///<summary>
        /// Check if item exists.
        ///</summary>
        ///<param name="unsavedDayInfo"></param>
        ///<returns></returns>
        bool Contains(IUnsavedDayInfo unsavedDayInfo);
        ///<summary>
        /// Unsaved days count.
        ///</summary>
        int Count { get; }
        ///<summary>
        /// Unsaved days.
        ///</summary>
        IList<IUnsavedDayInfo> UnsavedDays { get; }
        ///<summary>
        /// Unsaved days ordered by date, ASC.
        ///</summary>
        IList<IUnsavedDayInfo> UnsavedDaysOrderedByDate { get; }
        ///<summary>
        /// Contains a specific datetime regardless of scenario.
        ///</summary>
        ///<param name="dateTime"></param>
        ///<returns></returns>
        bool ContainsDateTime(DateOnly dateTime);
    }

    ///<summary>
    /// Unsaved day info
    ///</summary>
    public interface IUnsavedDayInfo : IEquatable<IUnsavedDayInfo>
    {
        ///<summary>
        /// Date time.
        ///</summary>
        DateOnly DateTime { get; }
        ///<summary>
        /// Scenario.
        ///</summary>
        IScenario Scenario { get; }
    }
}
