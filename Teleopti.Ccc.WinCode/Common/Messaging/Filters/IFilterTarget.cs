namespace Teleopti.Ccc.WinCode.Common.Messaging.Filters
{
    /// <summary>
    /// For handling filtering
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2009-06-02
    /// </remarks>
    public interface IFilterTarget
    {
        /// <summary>
        /// Adds the filter.
        /// </summary>
        /// <param name="replyOptionViewModel">The reply option view model.</param>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-06-02
        /// </remarks>
        void AddFilter(ReplyOptionViewModel replyOptionViewModel);

        /// <summary>
        /// Removes the filter if exists
        /// </summary>
        /// <param name="replyOptionViewModel">The reply option view model.</param>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-06-02
        /// </remarks>
        void RemoveFilter(ReplyOptionViewModel replyOptionViewModel);
    }
}
