using System;

namespace Teleopti.Interfaces.MessageBroker.Client
{
	public interface ISignalRClient : IDisposable, IMessageBrokerUrl
	{
		bool IsAlive { get; }
		void StartBrokerService(bool useLongPolling = false);
		void Call(string methodName, params object[] args);
		void RegisterCallbacks(Action<Notification> onNotification, Action afterConnectionCreated);
	}

	public interface IMessageBrokerUrl
	{
		void Configure(string url);
		string Url { get; }
	}

	public class MutableUrl : IMessageBrokerUrl
	{
		private string _url;

		public void Configure(string url)
		{
			_url = url;
		}

		public string Url { get { return _url; } }
	}
}