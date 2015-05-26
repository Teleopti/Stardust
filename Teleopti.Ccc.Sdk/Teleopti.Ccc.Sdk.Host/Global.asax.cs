﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Autofac;
using Autofac.Integration.Wcf;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.MultipleConfig;
using log4net;
using log4net.Config;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Config;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.WcfHost.Ioc;
using Teleopti.Ccc.Sdk.WcfService;
using Teleopti.Ccc.Sdk.WcfService.Factory;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.Sdk.WcfHost
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class Global : HttpApplication
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(Global));
		private static string _sitePath;
		private string _messageBrokerDisabledConfigurationValue;
		private bool _messageBrokerDisabled;
		private string _messageBrokerReceiveEnabledConfigurationValue;
		private bool _messageBrokerReceiveEnabled;

		protected void Application_End(object sender, EventArgs e)
		{
			Logger.Info("The Application ended.");

			var busSender = AutofacHostFactory.Container.Resolve<IServiceBusSender>();
			if (busSender != null)
				busSender.Dispose();
			if (AutofacHostFactory.Container != null)
				AutofacHostFactory.Container.Dispose();

			if (StateHolderReader.IsInitialized)
				StateHolder.Instance.Terminate();

		}

		protected void Application_Start(object sender, EventArgs e)
		{
			XmlConfigurator.Configure();

			Logger.InfoFormat("The Application is starting. {0}", _sitePath);

			var busSender = new ServiceBusSender();

			var builder = buildIoc();
			var container = builder.Build();
			AutofacHostFactory.Container = container;
			var messageBroker = container.Resolve<IMessageBrokerComposite>();
			var sharedSettingsQuerier = container.Resolve<ISharedSettingsQuerier>();

			var settings=sharedSettingsQuerier.GetSharedSettings();
			var appSettings = settings.AddToAppSettings(ConfigurationManager.AppSettings.ToDictionary());

			var passwordPolicyDocument = XDocument.Parse(settings.PasswordPolicy);
			var passwordPolicyService = new LoadPasswordPolicyService(passwordPolicyDocument);

			var populator = EventContextPopulator.Make();
			  var businessUnit = CurrentBusinessUnit.Make();
			var messageSender = new MessagePopulatingServiceBusSender(busSender, populator);
			var eventPublisher = new EventPopulatingPublisher(new ServiceBusEventPublisher(busSender), populator);
			var initializeApplication =
				new InitializeApplication(
					new DataSourcesFactory(new EnversConfiguration(),
						new List<IMessageSender>
						  {
							  new ScheduleMessageSender(eventPublisher, new ClearEvents()),
							  new EventsMessageSender(new SyncEventsPublisher(eventPublisher)),
							  new MeetingMessageSender(eventPublisher),
							  new GroupPageChangedMessageSender(messageSender),
							  new TeamOrSiteChangedMessageSender(eventPublisher, businessUnit),
							  new PersonChangedMessageSender(eventPublisher,businessUnit),
							  new PersonPeriodChangedMessageSender(messageSender)
						  },
						DataSourceConfigurationSetter.ForSdk(),
						new CurrentHttpContext(),
						() => StateHolderReader.Instance.StateReader.ApplicationScopeData.Messaging
						),
					messageBroker);
			//string sitePath = Global.sitePath();
			var messageBrokerEnabled = !messageBrokerDisabled();
			initializeApplication.Start(new SdkState(), appSettings, passwordPolicyService, messageBrokerEnabled);

			var messageBrokerReceiveDisabled = !messageBrokerReceiveEnabled();
			if (messageBrokerEnabled && messageBrokerReceiveDisabled)
				if (messageBroker != null)
					messageBroker.Dispose();

			Logger.Info("Initialized application");
		}

		private bool messageBrokerReceiveEnabled()
		{
			_messageBrokerReceiveEnabledConfigurationValue = ConfigurationManager.AppSettings["MessageBrokerReceiveEnabled"];
			if (!string.IsNullOrEmpty(_messageBrokerReceiveEnabledConfigurationValue))
			{
				if (!bool.TryParse(_messageBrokerReceiveEnabledConfigurationValue, out _messageBrokerReceiveEnabled))
					_messageBrokerReceiveEnabled = false;
			}
			return _messageBrokerReceiveEnabled;
		}

		private bool messageBrokerDisabled()
		{
			if (_messageBrokerDisabledConfigurationValue == null)
			{
				_messageBrokerDisabledConfigurationValue = ConfigurationManager.AppSettings["MessageBrokerDisabled"];
				if (!string.IsNullOrEmpty(_messageBrokerDisabledConfigurationValue))
					if (!bool.TryParse(_messageBrokerDisabledConfigurationValue, out _messageBrokerDisabled))
						_messageBrokerDisabled = false;
			}
			return _messageBrokerDisabled;
		}

		private static string sitePath()
		{
			if (_sitePath == null)
			{
				_sitePath = ConfigurationManager.AppSettings["SitePath"];
				if (string.IsNullOrEmpty(_sitePath.Trim()))
				{
					_sitePath = AppDomain.CurrentDomain.BaseDirectory;
				}
				Logger.InfoFormat("Read site path from configuration. {0}", _sitePath);
			}
			return _sitePath;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private static ContainerBuilder buildIoc()
		{
			var builder = new ContainerBuilder();

			var iocArgs = new IocArgs(new AppConfigReader());
			var configuration = new IocConfiguration(iocArgs, CommonModule.ToggleManagerForIoc(iocArgs));
			builder.RegisterModule(new CommonModule(configuration));
			builder.RegisterModule(new RuleSetModule(configuration, true));
			builder.RegisterModule<EncryptionModule>();
			builder.RegisterModule<PersonAccountModule>();
			builder.RegisterModule<AssemblerModule>();
			builder.RegisterModule<RequestFactoryModule>();
			builder.RegisterModule<QueryHandlerModule>();
			builder.RegisterModule<ShiftTradeModule>();
			builder.RegisterModule<SdkCommandHandlersModule>();
			builder.RegisterModule<CommandDispatcherModule>();
			builder.RegisterModule<CommandHandlersModule>();
			builder.RegisterModule<UpdateScheduleModule>();
			builder.RegisterModule<IntraIntervalSolverServiceModule>();
			builder.RegisterType<WebWindowsUserProvider>()
				 .As<IWindowsUserProvider>()
				 .InstancePerDependency();
			builder.RegisterModule(SchedulePersistModule.ForOtherModules());



			registerSdkFactories(builder, configuration);

			builder.RegisterType<LicenseCache>().As<ILicenseCache>();
			builder.RegisterType<MainShiftLayerConstructor>().As<ILayerConstructor<IMainShiftLayer>>().InstancePerLifetimeScope();
			builder.RegisterType<PersonalShiftLayerConstructor>().As<ILayerConstructor<IPersonalShiftLayer>>().InstancePerLifetimeScope();
			builder.RegisterType<UserCultureProvider>().As<IUserCultureProvider>().InstancePerLifetimeScope();
			builder.RegisterType<GroupingReadOnlyRepository>().As<IGroupingReadOnlyRepository>();

			return builder;
		}

		private static void registerSdkFactories(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterType<TeleoptiCccSdkService>();
			builder.RegisterType<GetPayrollResultById.MultiTenancyPayrollLogon>().As<GetPayrollResultById.IPayrollLogon>().InstancePerLifetimeScope();
			builder.RegisterModule(new MultiTenancyModule(configuration));
			builder.RegisterType<LicenseFactory>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleFactory>().InstancePerLifetimeScope();
			builder.RegisterType<TeleoptiPayrollExportFactory>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleMailFactory>().InstancePerLifetimeScope();
			builder.RegisterType<PublicNoteTypeFactory>().InstancePerLifetimeScope();
			builder.RegisterType<PersonsFromLoadOptionFactory>().InstancePerLifetimeScope();
			builder.RegisterType<FactoryProvider>().As<IFactoryProvider>().SingleInstance();
			builder.RegisterType<PayrollResultFactory>().As<IPayrollResultFactory>().SingleInstance();
			builder.RegisterType<PlanningTimeBankFactory>().InstancePerLifetimeScope();
			builder.RegisterType<WriteProtectionFactory>().InstancePerLifetimeScope();

			builder.RegisterType<ResourceCalculationPrerequisitesLoader>().As<IResourceCalculationPrerequisitesLoader>().InstancePerLifetimeScope();
			builder.RegisterType<SkillDayLoadHelper>().As<ISkillDayLoadHelper>().InstancePerLifetimeScope();
		}
	}

	

}
