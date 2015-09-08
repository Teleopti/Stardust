using System;
using System.Collections.Generic;
using System.Configuration;
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
using log4net;
using log4net.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.Config;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.Sdk.WcfHost.Ioc;
using Teleopti.Ccc.Sdk.WcfService;
using Teleopti.Ccc.Sdk.WcfService.Factory;
using Teleopti.Ccc.Sdk.WcfService.LogOn;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using ConfigReader = Teleopti.Ccc.Domain.Config.ConfigReader;

namespace Teleopti.Ccc.Sdk.WcfHost
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class Global : HttpApplication
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(Global));

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

			Logger.InfoFormat("The Application is starting. ");

			var busSender = new ServiceBusSender();

			var builder = BuildIoc();
			var container = builder.Build();
			AutofacHostFactory.Container = container;
			var messageBroker = container.Resolve<IMessageBrokerComposite>();
			var sharedSettingsQuerier = container.Resolve<ISharedSettingsQuerier>();

			var settings = sharedSettingsQuerier.GetSharedSettings();
			var appSettings = settings.AddToAppSettings(ConfigurationManager.AppSettings.ToDictionary());

			var passwordPolicyDocument = XDocument.Parse(settings.PasswordPolicy);
			var passwordPolicyService = new LoadPasswordPolicyService(passwordPolicyDocument);

			var populator = EventContextPopulator.Make();
			var businessUnit = CurrentBusinessUnit.Instance;
			var messageSender = new MessagePopulatingServiceBusSender(busSender, populator);
			var eventPublisher = new EventPopulatingPublisher(new ServiceBusEventPublisher(busSender), populator);
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
				senders.Add(new AggregatedScheduleChangeMessageSender(container.Resolve<Interfaces.MessageBroker.Client.IMessageSender>(),
					CurrentDataSource.Make(), businessUnit, container.Resolve<IJsonSerializer>(),
					container.Resolve<ICurrentInitiatorIdentifier>()));
			var messageSenders = new CurrentMessageSenders(senders);
			//temp hack - sometime in the future -> no tenant db dep here!
			var tenantUnitOfWorkManager = TenantUnitOfWorkManager.CreateInstanceForHostsWithOneUser(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);
			var dsForTenant = new DataSourceForTenant(new DataSourcesFactory(
							new EnversConfiguration(),
							messageSenders,
							DataSourceConfigurationSetter.ForSdk(),
							new CurrentHttpContext(),
							() => StateHolderReader.Instance.StateReader.ApplicationScopeData.Messaging
							));
			using (tenantUnitOfWorkManager.Start())
			{
				var initializeApplication =
					new InitializeApplication(dsForTenant, messageBroker, new LoadAllTenants(tenantUnitOfWorkManager));
				initializeApplication.Start(new SdkState(), passwordPolicyService, appSettings, true);
			}
			tenantUnitOfWorkManager.Dispose();

			Logger.Info("Initialized application");
		}

		public static ContainerBuilder BuildIoc()
		{
			var builder = new ContainerBuilder();

			var iocArgs = new IocArgs(new ConfigReader());
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

			//remove this crap when we get rid of local superuser in appdb
			builder.Register(c => 
				new tenantPeopleLoaderButSkipIfSuperUser_Hackish(new TenantPeopleLoader(c.Resolve<ITenantLogonDataManager>()), c.Resolve<ICurrentPersonContainer>()))
				.As<ITenantPeopleLoader>().InstancePerLifetimeScope();
		}

		private class tenantPeopleLoaderButSkipIfSuperUser_Hackish : ITenantPeopleLoader
		{
			private readonly TenantPeopleLoader _orgLoader;
			private readonly ICurrentPersonContainer _currentPersonContainer;

			public tenantPeopleLoaderButSkipIfSuperUser_Hackish(TenantPeopleLoader orgLoader, ICurrentPersonContainer currentPersonContainer)
			{
				_orgLoader = orgLoader;
				_currentPersonContainer = currentPersonContainer;
			}

			public void FillDtosWithLogonInfo(IList<PersonDto> personDtos)
			{
				if (_currentPersonContainer.Current().Person.Id.Value == SuperUser.Id_AvoidUsing_This)
					return;
				_orgLoader.FillDtosWithLogonInfo(personDtos);
			}
		}
	}
}
