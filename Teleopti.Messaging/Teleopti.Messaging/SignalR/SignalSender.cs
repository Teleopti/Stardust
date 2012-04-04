using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using SignalR.Client._20.Hubs;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Coders;
using Subscription = Teleopti.Interfaces.MessageBroker.Subscription;

namespace Teleopti.Messaging.SignalR
{
	public class SignalSender : IMessageSender
	{
		private readonly string _serverUrl;
		private IHubProxy _proxy;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#")]
		public SignalSender(string serverUrl)
		{
			_serverUrl = serverUrl;

			ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ignoreInvalidCertificate);
		}

		private static bool ignoreInvalidCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
		{
			return true;
		}

		public void Dispose()
		{
			_proxy = null;
		}

		public bool IsAlive
		{
			get { return _proxy != null; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public void SendRtaData(Guid personId, Guid businessUnitId, IExternalAgentState externalAgentState)
		{
			int sendAttempt = 0;
			while (sendAttempt < 3)
			{
				try
				{
					sendAttempt++;
					ExternalAgentStateEncoder encoder = new ExternalAgentStateEncoder();
					byte[] domainObject = encoder.Encode(externalAgentState);

					var type = typeof (IExternalAgentState);
					callProxy(new Notification
					          	{
					          		StartDate =
					          			Subscription.DateToString(externalAgentState.Timestamp.Add(externalAgentState.TimeInState.Negate())),
					          		EndDate = Subscription.DateToString(externalAgentState.Timestamp),
					          		DomainId = Subscription.IdToString(personId),
					          		DomainType = type.Name,
					          		DomainQualifiedType = type.AssemblyQualifiedName,
					          		ModuleId = Subscription.IdToString(Guid.Empty),
					          		DomainUpdateType = (int) DomainUpdateType.Insert,
					          		BinaryData = Encoding.UTF8.GetString(domainObject),
					          		BusinessUnitId = Subscription.IdToString(businessUnitId)
					          	});
					break;
				}
				catch (Exception)
				{
					InstantiateBrokerService();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4")]
		public void SendData(DateTime floor, DateTime ceiling, Guid moduleId, Guid domainObjectId, Type domainInterfaceType, DomainUpdateType updateType, string dataSource, Guid businessUnitId)
		{
			int sendAttempt = 0;
			while (sendAttempt < 3)
			{
				try
				{
					sendAttempt++;
					callProxy(new Notification
					          	{
					          		StartDate = Subscription.DateToString(floor),
					          		EndDate = Subscription.DateToString(ceiling),
					          		DomainId = Subscription.IdToString(domainObjectId),
					          		DomainType = domainInterfaceType.Name,
					          		ModuleId = Subscription.IdToString(moduleId),
					          		DomainUpdateType = (int) DomainUpdateType.Insert,
					          		DataSource = dataSource,
					          		BusinessUnitId = Subscription.IdToString(businessUnitId),
					          		BinaryData = null
					          	});
					break;
				}
				catch (Exception)
				{
					InstantiateBrokerService();
				}
			}
		}

		private void callProxy(Notification notification)
		{
			if (_proxy!=null)
			{
				_proxy.Invoke("NotifyClients", notification);
			}
		}

		public void InstantiateBrokerService()
		{
			var connection = new HubConnection(_serverUrl);
			_proxy = connection.CreateProxy("messageBrokerHub");
			
			connection.Start();
		}
	}
}