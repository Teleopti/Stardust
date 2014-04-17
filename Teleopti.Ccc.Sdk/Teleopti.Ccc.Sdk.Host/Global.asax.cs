using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using Autofac;
using Autofac.Integration.Wcf;
using Teleopti.Ccc.Domain.ApplicationLayer;
using MbCache.Configuration;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Messaging.SignalR;
using log4net;
using log4net.Config;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Config;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.WcfHost.Ioc;
using Teleopti.Ccc.Sdk.WcfService;
using Teleopti.Ccc.Sdk.WcfService.Factory;
using Teleopti.Interfaces.Domain;

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected void Application_Start(object sender, EventArgs e)
        {
            XmlConfigurator.Configure();

            Logger.InfoFormat("The Application is starting. {0}", _sitePath);

			var busSender = new ServiceBusSender();
	        var eventPublisher = new ServiceBusEventPublisher(busSender, new EventContextPopulator(new CurrentIdentity(), new CurrentInitiatorIdentifier(CurrentUnitOfWork.Make())));
	        var initializeApplication =
		        new InitializeApplication(
			        new DataSourcesFactory(new EnversConfiguration(),
			                               new List<IMessageSender>
				                               {
					                               new ScheduleMessageSender(eventPublisher, new ClearEvents()),
					                               new EventsMessageSender(new SyncEventsPublisher(eventPublisher)),
					                               new MeetingMessageSender(eventPublisher),
					                               new GroupPageChangedMessageSender(eventPublisher),
					                               new TeamOrSiteChangedMessageSender(eventPublisher),
					                               new PersonChangedMessageSender(eventPublisher),
					                               new PersonPeriodChangedMessageSender(eventPublisher)
				                               },
			                               DataSourceConfigurationSetter.ForSdk()),
			        new SignalBroker(MessageFilterManager.Instance)) {MessageBrokerDisabled = messageBrokerDisabled()};
            string sitePath = Global.sitePath();
            initializeApplication.Start(new SdkState(), sitePath, new LoadPasswordPolicyService(sitePath), new ConfigurationManagerWrapper(), true);
            var messageBroker = initializeApplication.MessageBroker;

            var messageBrokerEnabled = !messageBrokerDisabled();
            var messageBrokerReceiveDisabled = !messageBrokerReceiveEnabled();
            if (messageBrokerEnabled && messageBrokerReceiveDisabled)
                if (messageBroker != null)
                    messageBroker.StopMessageBroker();

            var container = buildIoc();
            AutofacHostFactory.Container = container.Build();
            
            Logger.Info("Initialized application");
        }

		private static IHandleCommand<DenormalizeNotificationCommandDto> denormalizeHandler()
		{
			return AutofacHostFactory.Container.Resolve<IHandleCommand<DenormalizeNotificationCommandDto>>();
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
			var mbCacheModule = new MbCacheModule(new InMemoryCache(20), null);

			builder.RegisterModule(mbCacheModule);
			builder.RegisterModule(new RuleSetModule(mbCacheModule, true));
			builder.RegisterModule<EncryptionModule>();
			builder.RegisterModule<AuthenticationModule>();
            builder.RegisterModule<PersonAccountModule>();
            builder.RegisterModule<AssemblerModule>();
            builder.RegisterModule<RepositoryModule>();
            builder.RegisterModule<UnitOfWorkModule>();
            builder.RegisterModule<RequestFactoryModule>();
            builder.RegisterModule<QueryHandlerModule>();
            builder.RegisterModule<ShiftTradeModule>();
			builder.RegisterModule<SdkCommandHandlersModule>();
			builder.RegisterModule<CommandDispatcherModule>();
			builder.RegisterModule<ServiceBusEventsPublisherModule>();
			builder.RegisterModule<CommandHandlersModule>();
			builder.RegisterModule<UpdateScheduleModule>();
			  builder.RegisterModule<DateAndTimeModule>();
            builder.RegisterType<WebWindowsUserProvider>()
                .As<IWindowsUserProvider>()
                .InstancePerDependency();
					builder.RegisterModule(SchedulePersistModule.ForOtherModules());

            registerSdkFactories(builder);

            builder.RegisterType<LicenseCache>().As<ILicenseCache>();
            builder.RegisterType<MainShiftLayerConstructor>().As<ILayerConstructor<IMainShiftLayer>>().InstancePerLifetimeScope();
            builder.RegisterType<PersonalShiftLayerConstructor>().As<ILayerConstructor<IPersonalShiftLayer>>().InstancePerLifetimeScope();
            builder.RegisterType<UserCultureProvider>().As<IUserCultureProvider>().InstancePerLifetimeScope();
            builder.RegisterType<GroupingReadOnlyRepository>().As<IGroupingReadOnlyRepository>();

            return builder;
        }

    	private static void registerSdkFactories(ContainerBuilder builder)
        {
            builder.RegisterType<TeleoptiCccSdkService>();
            builder.RegisterType<AuthenticationFactory>().InstancePerLifetimeScope();
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
            builder.RegisterType<PeopleAndSkillLoaderDecider>().As<IPeopleAndSkillLoaderDecider>().InstancePerLifetimeScope();
        }
    }
}
