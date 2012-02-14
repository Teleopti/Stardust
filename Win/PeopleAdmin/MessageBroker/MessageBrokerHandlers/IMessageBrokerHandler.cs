namespace Teleopti.Ccc.Win.PeopleAdmin.MessageBroker.MessageBrokerHandlers
{
    /// <summary>
    /// This provides handling insert, update, delete when event of message broker call back in given aggregate 
    /// root.
    /// </summary>
    /// <remarks>
    /// Created by: Dinesh Ranasinghe
    /// Created date: 2008-12-17
    /// </remarks>
    public interface IMessageBrokerHandler
    {
        /// <summary>
        /// Handles the insert when message broker call back.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-12-17
        /// </remarks>
        void HandleInsertFromMessageBroker();

        /// <summary>
        /// Handles the delete when message broker call back.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-12-17
        /// </remarks>
        void HandleDeleteFromMessageBroker();

        /// <summary>
        /// Handles the update when message broker call back.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-12-17
        /// </remarks>
        void HandleUpdateFromMessageBroker();
    }
}