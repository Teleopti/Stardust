using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Markup;
using Autofac;
using log4net;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.SmartClientPortal.Shell.Common.Constants;
using Teleopti.Ccc.SmartClientPortal.Shell.Common.Library;
using Teleopti.Ccc.SmartClientPortal.Shell.ConfigurationSections;
using Teleopti.Ccc.Win.Forecasting;
using log4net.Config;
using Microsoft.Practices.CompositeUI;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Win;
using Teleopti.Ccc.Win.Budgeting;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.ExceptionHandling;
using Teleopti.Ccc.Win.Grouping;
using Teleopti.Ccc.Win.Intraday;
using Teleopti.Ccc.Win.Main;
using Teleopti.Ccc.Win.Meetings.Overview;
using Teleopti.Ccc.Win.Permissions;
using Teleopti.Ccc.Win.Scheduling;
using Teleopti.Ccc.Win.Shifts;
using Teleopti.Ccc.WinCode.Autofac;
using Teleopti.Ccc.WinCode.Common.ExceptionHandling;
using Teleopti.Ccc.WinCode.Events;
using Teleopti.Ccc.WinCode.Main;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Application = System.Windows.Forms.Application;
using ConfigReader = Teleopti.Ccc.Domain.Config.ConfigReader;

namespace Teleopti.Ccc.SmartClientPortal.Shell
{
	/// <summary>
	/// Main application entry point class.
	/// Note that the class derives from CAB supplied base class FormSmartClientShellApplication, and the 
	/// main form will be SmartClientShellForm, also created by default by this solution template
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	class SmartClientShellApplication : SmartClientApplication<WorkItem, SmartClientShellForm>
	{

		/// <summary>
		/// Shell application entry point.
		/// </summary>
		[STAThread]
		private static void Main()
		{
			EO.WebBrowser.Runtime.AddLicense(
												"r6a3wN2vaqumsR70m7z8ARTxnurFBeihb6a3wN2vaq2msSHkq+rtABm8W6mm" +
	 "sdq9RoGkscufdert+Bngrez29unlgd7aCeO2rdvbyf73adru6vjmbM/Vzui7" +
	 "aOrt+Bngrez29umMQ7Oz/RTinuX39umMQ3Xj7fQQ7azcwp61n1mXpM0X6Jzc" +
	 "8gQQyJ21usPdtG2vuMrgtHWm8PoO5Kfq6doPvUaBpLHLn3Xj7fQQ7azc6c/n" +
	 "rqXg5/YZ8p7cwp61n1mXpM0M66Xm+8+4iVmXpLHLn1mXwPIP41nr/QEQvFu8" +
	 "07/u56vm8fbNn6/c9gQU7qe0psLgrWmZpMDpjEOXpLHLu2jY8P0a9neEjrHL" +
	 "n1mz8wMP5KvA8vcan53Y+PbooW8=");

			XmlConfigurator.Configure();

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			// WPF should use CurrentCulture
			FrameworkElement.LanguageProperty.OverrideMetadata(
				typeof(FrameworkElement),
				new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

			if(!createAppConfigReader())
				return;
			
			IContainer container = configureContainer();
#if (!DEBUG)
			 //NHibernateProfiler.Initialize();
			 SetReleaseMode();
			 populateFeatureToggleFlags_THISMUSTHAPPENBEFORELOGON_SEEBUG30359(container);
			 var applicationStarter = container.Resolve<ApplicationStartup>();
			 if (applicationStarter.LogOn())
			 {
				 try
				 {

					 applicationStarter.LoadShellApplication();
				 }
				 catch (Exception exception)
				 {
					 handleException(exception);
				 }
			 }

#endif

#if (DEBUG)
			populateFeatureToggleFlags_THISMUSTHAPPENBEFORELOGON_SEEBUG30359(container);
			var applicationStarter = container.Resolve<ApplicationStartup>();
			if (applicationStarter.LogOn())
			{
				applicationStarter.LoadShellApplication();
			}
#endif

		}

		private static void populateFeatureToggleFlags_THISMUSTHAPPENBEFORELOGON_SEEBUG30359(IContainer container)
		{
			try
			{
				var toggleFiller = container.ResolveOptional<IToggleFiller>();
				if (toggleFiller != null)
					toggleFiller.FillAllToggles();
			}
			catch (Exception ex)
			{
				using (var view = new SimpleExceptionHandlerView(ex, UserTexts.Resources.OpenTeleoptiCCC, toggleExceptionMessageBuilder()))
				{
					view.ShowDialog();
				}
				//if exception -> replace with a toggle manager returning false for everything
				var updater = new ContainerBuilder();
				updater.RegisterType<FalseToggleManager>().SingleInstance().As<IToggleManager>();
				updater.Update(container);
			}
		}

		private static string toggleExceptionMessageBuilder()
		{
			var ret = new StringBuilder();
			ret.AppendLine(UserTexts.Resources.WebServerDown);

			foreach (var toggle in Enum.GetValues(typeof(Toggles)))
			{
				var x = (Toggles)toggle;
				if (x != Toggles.TestToggle)
					ret.AppendLine(x.ToString());
			}

			return ret.ToString();
		}

		public SmartClientShellApplication(IComponentContext container)
			: base(container)
		{ }


		private static IConfigReader configReader;
		private static bool createAppConfigReader()
		{
			var appSettingsOverrides = ServerInstallations.FetchServerInstallations();
			if (appSettingsOverrides.IsEmpty())
			{
				configReader = new ConfigReader();
			}
			else
			{
				using (var preLogonView = new PreLogonScreen(appSettingsOverrides.Keys))
				{
					preLogonView.ShowDialog();
					if (preLogonView.DialogResult != DialogResult.OK)
						return false;

					configReader = new ConfigOverrider(new ConfigReader(), appSettingsOverrides[preLogonView.GetData()]);
				}
			}
			return true;
		}

		private static IContainer configureContainer()
		{
			using (PerformanceOutput.ForOperation("Building Ioc container"))
			{
				var builder = new ContainerBuilder();

				var iocArgs = new IocArgs(configReader) { MessageBrokerListeningEnabled = true };
				var configuration = new IocConfiguration(
							iocArgs,
							CommonModule.ToggleManagerForIoc(iocArgs));

				builder.RegisterModule(
				new CommonModule(configuration)
					{
						RepositoryConstructorType = typeof(IUnitOfWorkFactory)
					});
				builder.RegisterType<WinTenantCredentials>().As<ICurrentTenantCredentials>().SingleInstance();
				builder.RegisterModule<EncryptionModule>();
				builder.RegisterModule<EventAggregatorModule>();
				builder.RegisterModule(new StartupModule(configuration));
				builder.RegisterModule(new NavigationModule(configuration));
				builder.RegisterModule<BudgetModule>();
				builder.RegisterModule<IntradayModule>();
				builder.RegisterModule<ForecasterModule>();
				builder.RegisterModule<PersonAccountModule>();
				builder.RegisterModule(new ScheduleScreenRefresherModule(configuration));
				builder.RegisterModule<MeetingOverviewModule>();
				builder.RegisterModule<SchedulingServiceModule>();
				builder.RegisterModule(new RuleSetModule(configuration, true));
				builder.RegisterModule<ShiftsModule>();
				builder.RegisterModule(new PersonSelectorModule(configuration));
				builder.RegisterModule<PermissionsModule>();
				builder.RegisterModule<RequestHistoryModule>();
				builder.RegisterModule<MainModule>();
				builder.RegisterModule(new SchedulingCommonModule());
				builder.RegisterModule(new OutboundScheduledResourcesProviderModule());
				//hack to get old behavior work
				builder.Register(context => context.Resolve<ICurrentUnitOfWorkFactory>().Current()).ExternallyOwned().As<IUnitOfWorkFactory>();
				builder.RegisterType<CurrentUnitOfWorkFactory>().As<ICurrentUnitOfWorkFactory>().SingleInstance();
				//////
				builder.Register(c => new WebConfigReader(() =>
				{
					var webSettings = new WebSettings
					{
						Settings =
							c.Resolve<ISharedSettingsQuerier>()
								.GetSharedSettings()
								.AddToAppSettings(ConfigurationManager.AppSettings.ToDictionary())
					};
					return webSettings;
				})).As<IConfigReader>().SingleInstance();
				return builder.Build();
			}
		}

		/// <summary>
		/// Runs the application in release mode.
		/// </summary>
		private static void SetReleaseMode()
		{
			AppDomain.CurrentDomain.UnhandledException += appDomainUnhandledException;
			Application.ThreadException += applicationThreadException;
		}

		/// <summary>
		/// Sets the extension site registration after the SmartClientShell has been created.
		/// </summary>
		protected override void AfterShellCreated()
		{
			base.AfterShellCreated();

			RootWorkItem.UIExtensionSites.RegisterSite(UIExtensionSiteNames.MainStatus, Shell.MainStatusStrip);
		}

		/// <summary>
		/// Handles the unhandled exception event in the application.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.UnhandledExceptionEventArgs"/> instance containing the event data.</param>
		private static void appDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			handleException(e.ExceptionObject as Exception);
		}

		private static void applicationThreadException(object sender, ThreadExceptionEventArgs e)
		{
			handleException(e.Exception);
		}

		private static void handleException(Exception ex)
		{
			if (ex == null) return;

			Application.ThreadException -= applicationThreadException;
			AppDomain.CurrentDomain.UnhandledException -= appDomainUnhandledException;
			string fallBack = string.Empty;

			var iocArgs = new IocArgs(configReader);
			var tempContainerBecauseWeDontHaveAGlobalOneHere = new ContainerBuilder();
			tempContainerBecauseWeDontHaveAGlobalOneHere.RegisterModule(new CommonModule(new IocConfiguration(iocArgs, CommonModule.ToggleManagerForIoc(iocArgs))));
			ITogglesActive toggles;
			using (var container = tempContainerBecauseWeDontHaveAGlobalOneHere.Build())
			{
				try
				{
					toggles = container.Resolve<ITogglesActive>();
				}
				catch (Exception)
				{
					toggles = new emptyStubOfActivteToggles_OnlyUseIfWebCannotBeReached();
				}
			}
			var exceptionMessageBuilder = new ExceptionMessageBuilder(ex, toggles);

			try
			{
				var log = LogManager.GetLogger(typeof(SmartClientShellApplication));
				log.Error(exceptionMessageBuilder.BuildSimpleExceptionMessage(), ex);
			}
			catch (Exception)
			{
				//do nothing
			}

			var emailSetting = new StringSetting();
			if (StateHolderReader.IsInitialized)
			{
				try
				{
					using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
					{
						ISettingDataRepository settingDataRepository = new GlobalSettingDataRepository(uow);
						emailSetting = settingDataRepository.FindValueByKey("SupportEmailSetting", new StringSetting());
					}
				}
				catch (Exception)
				{
					fallBack = " ";
				}
			}

			string defaultEmail = !string.IsNullOrEmpty(emailSetting.StringValue)
											  ? emailSetting.StringValue
											  : fallBack;
			var fileWriter = new WriteStringToFile();
			var message = new MapiMailMessage(string.Empty, string.Empty);
			var exceptionHandlerModel = new ExceptionHandlerModel(ex, defaultEmail, message, fileWriter, exceptionMessageBuilder);
			using (var view = new ExceptionHandlerView(exceptionHandlerModel))
			{
				view.ShowDialog();
			}
			killOpenForms();
			Application.Exit();

		}

		private class emptyStubOfActivteToggles_OnlyUseIfWebCannotBeReached: ITogglesActive
		{
			public IDictionary<Toggles, bool> AllActiveToggles()
			{
				return new Dictionary<Toggles, bool>();
			}
		}

		private static void killOpenForms()
		{
			ArrayList forms = new ArrayList(Application.OpenForms);
			for (int i = 0; i < forms.Count; i++)
			{
				BaseRibbonForm baseRibbonFormToClose = forms[i] as BaseRibbonForm;
				if (baseRibbonFormToClose != null)
				{
					baseRibbonFormToClose.FormKill();
				}
			}
		}
	}
}
