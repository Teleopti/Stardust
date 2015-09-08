using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml.Linq;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Config;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.WinCode.Common.ServiceBus;
using Teleopti.Ccc.WinCode.Services;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.WinCode.Main
{
	public static class LogonInitializeStateHolder
	{
		public static void InitWithoutDataSource(IMessageBrokerComposite messageBroker, SharedSettings settings,
			IComponentContext container)
		{
			LoadPasswordPolicyService passwordPolicyService;
			if (settings.PasswordPolicy == null)
			{
				//to be able start desktop app without shared setting server
				passwordPolicyService = new LoadPasswordPolicyService(Environment.CurrentDirectory);
			}
			else
			{
				var passwordPolicyDocument = XDocument.Parse(settings.PasswordPolicy);
				passwordPolicyService = new LoadPasswordPolicyService(passwordPolicyDocument);
			}

			var appSettings = settings.AddToAppSettings(ConfigurationManager.AppSettings.ToDictionary());
			var sendToServiceBus = string.IsNullOrEmpty(ConfigurationManager.AppSettings["FreemiumForecast"])
				? (IServiceBusSender) new ServiceBusSender()
				: new NoServiceBusSender();
			var populator = EventContextPopulator.Make();
			var businessUnit = CurrentBusinessUnit.Instance;
			var messageSender = new MessagePopulatingServiceBusSender(sendToServiceBus, populator);
			var eventPublisher = new EventPopulatingPublisher(new ServiceBusEventPublisher(sendToServiceBus), populator);
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
			if (container.Resolve<IToggleManager>().IsEnabled(Toggles.MessageBroker_SchedulingScreenMailbox_32733))
				senders.Add(new AggregatedScheduleChangeMessageSender(
					messageBroker,
					CurrentDataSource.Make(),
					businessUnit,
					new NewtonsoftJsonSerializer(),
					new CurrentInitiatorIdentifier(CurrentUnitOfWork.Make()))
					);
			var messageSenders = new CurrentMessageSenders(senders);
			var dsForTenant = new DataSourceForTenant(new DataSourcesFactory(
				new EnversConfiguration(),
				messageSenders,
				DataSourceConfigurationSetter.ForDesktop(),
				new CurrentHttpContext(),
				() => messageBroker));
			var initializer = new InitializeApplication(dsForTenant, messageBroker, new NoTenants());

			initializer.Start(new StateManager(), passwordPolicyService, appSettings, true);
		}
	}
}