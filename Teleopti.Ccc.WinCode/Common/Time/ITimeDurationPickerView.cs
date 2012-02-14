using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.WinCode.Common.Time
{
    public interface ITimeDurationPickerView
    {
        /// <summary>
        /// Gets or sets the interval.
        /// </summary>
        /// <value>The interval.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-05-27
        /// </remarks>
        [Browsable(true), Category("Custom settings"), DefaultValue("00:30")]
        TimeSpan Interval { get; set; }


        /// <summary>
        /// Sets the time list.
        /// </summary>
        /// <param name="timeSpans">The time spans.</param>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2010-06-14
        /// </remarks>
        void SetTimeList(IList<TimeSpanDataBoundItem> timeSpans);
    }
}
