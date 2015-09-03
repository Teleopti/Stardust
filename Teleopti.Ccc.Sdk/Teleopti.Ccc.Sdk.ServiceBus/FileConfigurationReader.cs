using System;
using System.Collections.Generic;
using System.Configuration;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using log4net;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class FileConfigurationReader : IConfigurationReader
	{
		private readonly ILoadAllTenants _loadAllTenants;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(FileConfigurationReader));
		private static readonly object LockObject = new object();

		public FileConfigurationReader(ILoadAllTenants loadAllTenants)
		{
			_loadAllTenants = loadAllTenants;
		}

		public void ReadConfiguration(MessageSenderCreator creator, Func<IMessageBrokerComposite> messageBroker)
		{
			lock (LockObject)
			{
				if (StateHolderReader.IsInitialized)
				{
					Logger.Info("StateHolder already initialized. This step is skipped.");
					return;
				}

				var application =
					new InitializeApplication(
						new DataSourcesFactory(new EnversConfiguration(), 
							creator.Create(),
							DataSourceConfigurationSetter.ForServiceBus(),
							new CurrentHttpContext(),
							messageBroker),
						messageBroker(),
						_loadAllTenants);
				application.Start(new BasicState(), null, ConfigurationManager.AppSettings.ToDictionary(), true);

				Logger.Info("Initialized application");
			}
		}
	}

	public class InternalServiceBusSender : IServiceBusSender
	{
		private readonly Func<IServiceBus> _serviceBus;

		public InternalServiceBusSender(Func<IServiceBus> serviceBus)
		{
			_serviceBus = serviceBus;
		}

		public void Dispose()
		{
		}

		public void Send(bool throwOnNoBus, params object[] message)
		{
			_serviceBus().Send(message);
		}

		public bool EnsureBus()
		{
			return true;
		}
	}

	public class MessageSenderCreator
	{
		private readonly IServiceBusSender _serviceBusSender;
		private readonly IToggleManager _toggleManager;
		private readonly Interfaces.MessageBroker.Client.IMessageSender _messageSender;
		private readonly IJsonSerializer _serializer;
		private readonly ICurrentInitiatorIdentifier _initiatorIdentifier;

		public MessageSenderCreator(IServiceBusSender serviceBusSender, 
			IToggleManager toggleManager, 
			Interfaces.MessageBroker.Client.IMessageSender messageSender,
			IJsonSerializer serializer, 
			ICurrentInitiatorIdentifier initiatorIdentifier)
		{
			_serviceBusSender = serviceBusSender;
			_toggleManager = toggleManager;
			_messageSender = messageSender;
			_serializer = serializer;
			_initiatorIdentifier = initiatorIdentifier;
		}

		public ICurrentMessageSenders Create()
		{
			var populator = EventContextPopulator.Make();
			var businessUnit = CurrentBusinessUnit.InstanceFromContainer;
			var messageSender = new MessagePopulatingServiceBusSender(_serviceBusSender, populator);
			var eventPublisher = new EventPopulatingPublisher(new ServiceBusEventPublisher(_serviceBusSender), populator);
			var senders = new List<IMessageSender>
			{
				new ScheduleMessageSender(eventPublisher, new ClearEvents()),
				new EventsMessageSender(new SyncEventsPublisher(eventPublisher)),
				new MeetingMessageSender(eventPublisher),
				new GroupPageChangedMessageSender(messageSender),
				new TeamOrSiteChangedMessageSender(eventPublisher, businessUnit),
				new PersonChangedMessageSender(eventPublisher, businessUnit),
				new PersonPeriodChangedMessageSender(messageSender)
			};
			if (_toggleManager.IsEnabled(Toggles.MessageBroker_SchedulingScreenMailbox_32733))
				senders.Add(new AggregatedScheduleChangeMessageSender(_messageSender, CurrentDataSource.Make(), businessUnit, _serializer,
					_initiatorIdentifier));
			return new CurrentMessageSenders(senders);
		}
	}

	public interface IConfigurationReader
	{
		void ReadConfiguration(MessageSenderCreator creator, Func<IMessageBrokerComposite> messageBroker);
	}
}
