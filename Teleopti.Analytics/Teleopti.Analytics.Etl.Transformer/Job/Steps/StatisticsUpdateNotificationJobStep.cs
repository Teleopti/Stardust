using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Exceptions;
using log4net;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
	public class StatisticsUpdateNotificationJobStep : JobStepBase
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(StatisticsUpdateNotificationJobStep));

		public StatisticsUpdateNotificationJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "Statistics Update Notification";
			IsBusinessUnitIndependent = true;
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			//Send to MessageBroker that new queue stats is loaded
			var messageSender = _jobParameters.Helper.MessageSender;
			try
			{
				if (!messageSender.IsAlive)
					messageSender.StartBrokerService();
				if (messageSender.IsAlive)
				{
					var identity = (ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity;
					
					var notification = new Notification
					{
						StartDate = Subscription.DateToString(DateTime.Now.Date),
						EndDate = Subscription.DateToString(DateTime.Now.Date),
						DomainId = Subscription.IdToString(Guid.Empty),
						DomainQualifiedType = typeof(IStatisticTask).AssemblyQualifiedName,
						DomainType = typeof(IStatisticTask).Name,
						ModuleId = Subscription.IdToString(Guid.NewGuid()),
						DomainUpdateType = (int)DomainUpdateType.Insert,
						DataSource = identity.DataSource.DataSourceName,
						BusinessUnitId = Subscription.IdToString(Guid.Empty),
						BinaryData = null
					};

					messageSender.SendNotification(notification);
				}
			}
			catch (BrokerNotInstantiatedException exception)
			{
				Logger.Error("An error occured while trying to notify clients via Message Broker.", exception);
			}
			catch (SocketException socketException)
			{
				Logger.Error("An error occured while trying to notify clients via Message Broker.", socketException);
			}

			return 0;
		}
	}
}