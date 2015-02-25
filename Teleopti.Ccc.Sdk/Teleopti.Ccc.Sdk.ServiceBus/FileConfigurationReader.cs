using System;
using System.Collections.Generic;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using log4net;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class FileConfigurationReader : IConfigurationReader
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(FileConfigurationReader));
		private readonly string _xmlFilePath;
		private static readonly object LockObject = new object();

		public FileConfigurationReader(string xmlFilePath)
		{
			_xmlFilePath = xmlFilePath;
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
						new DataSourcesFactory(new EnversConfiguration(), creator.Create(),
							DataSourceConfigurationSetter.ForServiceBus(), new CurrentHttpContext()),
						messageBroker());
				application.Start(new BasicState(), _xmlFilePath, null, new ConfigurationManagerWrapper(), true);

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

		public void Send(object message, bool throwOnNoBus)
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

		public MessageSenderCreator(IServiceBusSender serviceBusSender)
		{
			_serviceBusSender = serviceBusSender;
		}

		public IList<IMessageSender> Create()
		{
			var populator = EventContextPopulator.Make();
			var messageSender = new MessagePopulatingServiceBusSender(_serviceBusSender, populator);
			var eventPublisher = new EventPopulatingPublisher(new ServiceBusEventPublisher(_serviceBusSender), populator);
			return new List<IMessageSender>
				{
					new ScheduleMessageSender(eventPublisher, new ClearEvents()),
					new EventsMessageSender(new SyncEventsPublisher(eventPublisher)),
					new MeetingMessageSender(eventPublisher),
					new GroupPageChangedMessageSender(messageSender),
					new TeamOrSiteChangedMessageSender(messageSender),
					new PersonChangedMessageSender(messageSender),
					new PersonPeriodChangedMessageSender(messageSender)
				};
		}
	}

	public class ConfigurationReaderFactory
	{
		public IConfigurationReader Reader()
		{
			var wrapper = new ConfigurationManagerWrapper();
			var xmlPath = wrapper.AppSettings["ConfigPath"];
			if (string.IsNullOrWhiteSpace(xmlPath))
			{
				xmlPath = AppDomain.CurrentDomain.BaseDirectory;
			}

			return new FileConfigurationReader(xmlPath);
		}
	}

	public interface IConfigurationReader
	{
		void ReadConfiguration(MessageSenderCreator creator, Func<IMessageBrokerComposite> messageBroker);
	}
}
