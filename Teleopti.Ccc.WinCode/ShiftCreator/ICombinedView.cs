using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.ShiftCreator
{
    /// <summary>
    /// 
    /// </summary>
    public enum CombinedViewType
    {
        /// <summary>
        /// 
        /// </summary>
        AutoPosition,
        /// <summary>
        /// 
        /// </summary>
        AbsolutePosition,
    }

    /// <summary>
    /// 
    /// </summary>
    public class CombinedViewTypeChangeEventArgs : EventArgs
    {
        private CombinedViewType _type;
        private ICombinedView _item;

        /// <summary>
        /// 
        /// </summary>
        public CombinedViewTypeChangeEventArgs(CombinedViewType type, ICombinedView item) 
            : base()
        {
            _type = type;
            _item = item;
        }

        /// <summary>
        /// 
        /// </summary>
        public CombinedViewType CombinedViewType
        {
            get { return _type; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICombinedView Item
        {
            get { return _item; }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface ICombinedView : IGridDataValidator
    {
        /// <summary>
        /// 
        /// </summary>
        WorkShiftRuleSet RuleSet { get; set; }

        /// <summary>
        /// 
        /// </summary>
        bool IsAutoPosition { get; set; }

        /// <summary>
        /// 
        /// </summary>
        object CVCount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        Type TypeClass { get; }

        /// <summary>
        /// 
        /// </summary>
        string CurrentActivity { get; set; }

        /// <summary>
        /// 
        /// </summary>
        TimeSpan ALSegment { get; set; }

        /// <summary>
        /// /
        /// </summary>
        TimeSpan ALMinTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        TimeSpan ALMaxTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        TimeSpan APSegment { get; set; }

        /// <summary>
        /// 
        /// </summary>
        TimeSpan? APStartTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        TimeSpan? APEndTime { get; set; }

        IWorkShiftExtender WorkShiftExtender { get;  }

        WorkShiftRuleSet WorkShiftRuleSet { get; set;  }

        event EventHandler<CombinedViewTypeChangeEventArgs> TypeChanged;
    }
}
