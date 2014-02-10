using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Teleopti.Messaging.Exceptions;
using log4net;

namespace Teleopti.Messaging.SignalR
{
	public class SignalSenderBase
	{
		[CLSCompliant(false)]
		protected string ServerUrl;
		[CLSCompliant(false)]
		protected ISignalWrapper Wrapper;
		[CLSCompliant(false)]
		protected ILog Logger;

		public SignalSenderBase(string serverUrl)
		{
			ServerUrl = serverUrl;

			ServicePointManager.ServerCertificateValidationCallback = IgnoreInvalidCertificate;
			ServicePointManager.DefaultConnectionLimit = 50;

			TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
		}

		public bool IsAlive
		{
			get { return Wrapper != null && Wrapper.IsInitialized(); }
		}

		protected static bool IgnoreInvalidCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
		{
			return true;
		}

		protected void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
		{
			if (e.Observed) return;
			Logger.Error("An error occured, please review the error and take actions necessary.", e.Exception);
			e.SetObserved();
		}

		public void StartBrokerService()
		{
			try
			{
				var connection = MakeHubConnection();
				var proxy = connection.CreateHubProxy("MessageBrokerHub");

				Wrapper = Wrapper ?? new SignalWrapper(proxy, connection, Logger);
				Wrapper.StartHub();
			}
			catch (SocketException exception)
			{
				Logger.Error("The message broker seems to be down.", exception);
			}
			catch (BrokerNotInstantiatedException exception)
			{
				Logger.Error("The message broker seems to be down.", exception);
			}
		}

		[CLSCompliant(false)]
		protected virtual IHubConnectionWrapper MakeHubConnection()
		{
			return new HubConnectionWrapper(new HubConnection(ServerUrl));
		}
	}
}