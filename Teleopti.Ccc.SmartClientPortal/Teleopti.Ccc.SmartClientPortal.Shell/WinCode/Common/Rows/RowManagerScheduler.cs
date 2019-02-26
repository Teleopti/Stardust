using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.WinCode.Scheduling;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows
{
    /// <summary>
    /// RowManager used by Scheduler to be able to set timeZoneInfo
    /// </summary>
    /// <typeparam name="TRow"></typeparam>
    /// <typeparam name="TSource"></typeparam>
    public class RowManagerScheduler<TRow, TSource> : RowManager<TRow, TSource> where TRow : IGridRow
    {

        private readonly ISchedulerStateHolder _schedulerStateHolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="RowManagerScheduler&lt;TRow, TSource&gt;"/> class.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="intervals">The intervals.</param>
        /// <param name="intervalLength">Length of the interval.</param>
        /// <param name="schedulerStateHolder">The scheduler state holder.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-03-22
        /// </remarks>
        public RowManagerScheduler(ITeleoptiGridControl grid, IList<IntervalDefinition> intervals, int intervalLength, ISchedulerStateHolder schedulerStateHolder) : base(grid, intervals, intervalLength)
        {
            _schedulerStateHolder = schedulerStateHolder;
        }

        /// <summary>
        /// Gets the time zone info.
        /// </summary>
        /// <value>The time zone info.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-03-21
        /// </remarks>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-03-22
        /// </remarks>
        public override TimeZoneInfo TimeZoneInfo
        {
            get
            {
                return TimeZoneGuardForDesktop_DONOTUSE.Instance_DONTUSE.CurrentTimeZone();
            }
        }

        public ISchedulerStateHolder SchedulerStateHolder
        {
            get { return _schedulerStateHolder; }
        }
    }
}
