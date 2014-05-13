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
		/// Syncronous sending of single notification 
		/// </summary>
		/// <param name="notification"></param>
		void SendNotification(Notification notification);

	}
}