using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces
{
    public enum ActivityExtenderType
    {
        ActivityAbsoluteStartExtender,
        ActivityRelativeStartExtender,
        ActivityRelativeEndExtender,
    }

    public interface IActivityViewModel : IBaseModel
    {
        bool IsAutoPosition { get; set; }

        object Count { get; set; }

        IActivity CurrentActivity { get; set; }

        TimeSpan ALSegment { get; set; }

        TimeSpan ALMinTime { get; set; }

        TimeSpan ALMaxTime { get; set; }

        TimeSpan APSegment { get; set; }

        TimeSpan? APStartTime { get; set; }

        TimeSpan? APEndTime { get; set; }

        Type TypeOfClass { get; }

        bool IsTimeOfDay { get; }

        IWorkShiftExtender WorkShiftExtender { get; }

        event EventHandler<ActivityTypeChangedEventArgs> ActivityTypeChanged;
    }

    public interface IActivityViewModel<T> : IActivityViewModel, IBaseModel<T> where T : IWorkShiftExtender
    {}
}
