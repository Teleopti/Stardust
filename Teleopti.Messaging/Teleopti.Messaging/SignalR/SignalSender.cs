using System;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Events;
using log4net;
using Subscription = Teleopti.Interfaces.MessageBroker.Subscription;

namespace Teleopti.Messaging.SignalR
{
	public class SignalSender : IMessageSender
	{
		private readonly string _serverUrl;
		private ISignalWrapper _wrapper;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#")]
		public SignalSender(string serverUrl)
		{
			_serverUrl = serverUrl;

			ServicePointManager.ServerCertificateValidationCallback = ignoreInvalidCertificate;
			ServicePointManager.DefaultConnectionLimit = 50;

            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
		}

	    private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
	    {
	        if (!e.Observed)
            {
                var logger = LogManager.GetLogger(typeof(SignalSender));
            
                logger.Error("An error occured, please review the error and take actions necessary.", e.Exception);
                e.SetObserved();
            }
	    }

	    private static bool ignoreInvalidCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
		{
			return true;
		}

		public void Dispose()
		{
			_wrapper.StopListening();
			_wrapper = null;
		}

		public bool IsAlive
		{
			get { return _wrapper != null && _wrapper.IsInitialized(); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public void SendRtaData(Guid personId, Guid businessUnitId, IActualAgentState actualAgentState)
		{
			int sendAttempt = 0;
			while (sendAttempt < 3)
			{
				try
				{
					sendAttempt++;
					var domainObject = JsonConvert.SerializeObject(actualAgentState);
                    var type = typeof(IActualAgentState);
					_wrapper.NotifyClients(new Notification
								{
									StartDate =
										Subscription.DateToString(actualAgentState.ReceivedTime.Add(actualAgentState.TimeInState.Negate())),
									EndDate = Subscription.DateToString(actualAgentState.ReceivedTime),
									DomainId = Subscription.IdToString(personId),
									DomainType = type.Name,
									DomainQualifiedType = type.AssemblyQualifiedName,
									ModuleId = Subscription.IdToString(Guid.Empty),
									DomainUpdateType = (int)DomainUpdateType.Insert,
					          		BinaryData = Convert.ToBase64String(Encoding.UTF8.GetBytes(domainObject)),
									BusinessUnitId = Subscription.IdToString(businessUnitId)
								});
					break;
				}
				catch (Exception)
				{
					InstantiateSendOnlyBrokerService();
				}
			}
		}

		public void InstantiateSendOnlyBrokerService()
		{
			var connection = new HubConnection(_serverUrl);
			var proxy = connection.CreateHubProxy("MessageBrokerHub");
			_wrapper = new SignalSendOnlyWrapper(proxy, connection);
			_wrapper.StartListening();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4")]
		public void SendData(DateTime floor, DateTime ceiling, Guid moduleId, Guid domainObjectId, Type domainInterfaceType, DomainUpdateType updateType, string dataSource, Guid businessUnitId)
		{
			var sendAttempt = 0;
			while (sendAttempt < 3)
			{
				try
				{
					sendAttempt++;

					var task = _wrapper.NotifyClients(new Notification
					          	{
					          		StartDate = Subscription.DateToString(floor),
					          		EndDate = Subscription.DateToString(ceiling),
					          		DomainId = Subscription.IdToString(domainObjectId),
									DomainQualifiedType = domainInterfaceType.AssemblyQualifiedName,
					          		DomainType = domainInterfaceType.Name,
					          		ModuleId = Subscription.IdToString(moduleId),
					          		DomainUpdateType = (int) DomainUpdateType.Insert,
					          		DataSource = dataSource,
					          		BusinessUnitId = Subscription.IdToString(businessUnitId),
					          		BinaryData = null
					          	});
					task.Wait(TimeSpan.FromSeconds(20));
					break;
				}
				catch (Exception)
				{
					InstantiateBrokerService();
				}
			}
		}

		public void InstantiateBrokerService()
		{
			var connection = new HubConnection(_serverUrl);
			var proxy = connection.CreateHubProxy("MessageBrokerHub");

			_wrapper = new SignalWrapper(proxy, connection);
			_wrapper.StartListening();
		}
	}
}