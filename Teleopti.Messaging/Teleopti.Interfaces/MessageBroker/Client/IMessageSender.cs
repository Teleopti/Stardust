namespace Teleopti.Interfaces.MessageBroker.Client
{
	/// <summary>
	/// Syncronous Message sender
	/// </summary>
	public interface IMessageSender
	{
		/// <summary>
		/// Indicator if instance is alive
		/// </summary>
		bool IsAlive { get; }

		/// <summary>
		/// Starts the broker
		/// </summary>
		void StartBrokerService();

		/// <summary>
		/// Starts the broker with max number of retries set
		/// </summary>
		/// <param name="reconnectAttempts">How many times connection should try to reconnect</param>
		void StartBrokerService(int reconnectAttempts);

		/// <summary>
		/// Syncronous sending of single notification 
		/// </summary>
		/// <param name="notification"></param>
		void SendNotification(Notification notification);

	}
}