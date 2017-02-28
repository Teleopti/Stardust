namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for objects containing tasks directly or in-directly
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-12-18
    /// </remarks>
    public interface ITaskSource
    {
        /// <summary>
        /// Called when [average task time changed].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-18
        /// </remarks>
        void OnAverageTaskTimesChanged();

        /// <summary>
        /// Called when [tasks changed].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-18
        /// </remarks>
        void OnTasksChanged();

        /// <summary>
        /// Called when [campaign tasks changed].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        void OnCampaignTasksChanged();

        /// <summary>
        /// Called when [campaign average times changed].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        void OnCampaignAverageTimesChanged();
    }
}