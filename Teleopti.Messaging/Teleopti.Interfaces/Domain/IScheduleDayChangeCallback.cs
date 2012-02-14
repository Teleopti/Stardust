namespace Teleopti.Interfaces.Domain
{
    ///<summary>
    /// Callback that gets called before and after schedule changes.
    ///</summary>
    public interface IScheduleDayChangeCallback
    {
        ///<summary>
        /// Schedule before change.
        ///</summary>
        ///<param name="partBefore">The schedule before changes.</param>
        void ScheduleDayChanging(IScheduleDay partBefore);

        ///<summary>
        /// Schedule after change.
        ///</summary>
        ///<param name="partAfter">The schedule after changes.</param>
        void ScheduleDayChanged(IScheduleDay partAfter);
    }
}