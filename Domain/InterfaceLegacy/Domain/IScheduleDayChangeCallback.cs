namespace Teleopti.Interfaces.Domain
{
    ///<summary>
    /// Callback that gets called before and after schedule changes.
    ///</summary>
    public interface IScheduleDayChangeCallback
    {
        void ScheduleDayBeforeChanging();

        void ScheduleDayChanged(IScheduleDay partBefore, IScheduleDay partAfter);
    }
}