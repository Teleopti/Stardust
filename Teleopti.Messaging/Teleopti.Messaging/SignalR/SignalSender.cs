﻿using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json;
using Microsoft.AspNet.SignalR.Client.Hubs;
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
		private SignalWrapper _wrapper;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#")]
		public SignalSender(string serverUrl)
		{
			_serverUrl = serverUrl;

			ServicePointManager.ServerCertificateValidationCallback = ignoreInvalidCertificate;
			ServicePointManager.DefaultConnectionLimit = 50;
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
					//ExternalAgentStateEncoder encoder = new ExternalAgentStateEncoder();
					//byte[] domainObject = encoder.Encode(externalAgentState);
					var domainObject = JsonConvert.SerializeObject(actualAgentState);
                    var type = typeof(IActualAgentState);
					//var type = typeof(IActualAgentState);
					_wrapper.NotifyClients(new Notification
								{
									StartDate =
										Subscription.DateToString(actualAgentState.Timestamp.Add(actualAgentState.TimeInState.Negate())),
									EndDate = Subscription.DateToString(actualAgentState.Timestamp),
									DomainId = Subscription.IdToString(personId),
									DomainType = type.Name,
									DomainQualifiedType = type.AssemblyQualifiedName,
									ModuleId = Subscription.IdToString(Guid.Empty),
									DomainUpdateType = (int)DomainUpdateType.Insert,
									BinaryData = domainObject,
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