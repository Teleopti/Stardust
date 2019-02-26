using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Markup;
using Autofac;
using EO.WebBrowser;
using log4net;
using log4net.Config;
using Microsoft.Practices.CompositeUI;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.SmartClientPortal.Shell.Common.Constants;
using Teleopti.Ccc.SmartClientPortal.Shell.Common.Library;
using Teleopti.Ccc.SmartClientPortal.Shell.ConfigurationSections;
using Teleopti.Ccc.SmartClientPortal.Shell.Win;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Budgeting;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.ExceptionHandling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Grouping;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Intraday;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Main;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Meetings.Overview;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Shifts;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ExceptionHandling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Main;
using Teleopti.Ccc.WinCode.Scheduling;
using Application = System.Windows.Forms.Application;

namespace Teleopti.Ccc.SmartClientPortal.Shell
{
	public static class ContainerForLegacy
	{
		public static IContainer Container;
	}

	/// <summary>
	/// Main application entry point class.
	/// Note that the class derives from CAB supplied base class FormSmartClientShellApplication, and the 
	/// main form will be SmartClientShellForm, also created by default by this solution template
	/// </summary>
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	class SmartClientShellApplication : SmartClientApplication<WorkItem, SmartClientShellForm>
	{
		/// <summary>
		/// Shell application entry point.
		/// </summary>
		[STAThread]
		private static void Main()
		{
			Runtime.AddLicense(
				"wJ7qxQXooW+mt8Ddr2qvprEh5Kvq7QAZvFupprHavUaBpLHLn3Xq7fgZ4K3s" +
				"9vbp732v+AvxsbGn3dUE9K2i0d8O9oeo+87ou2jq7fgZ4K3s9vbpjEOzs/0U" +
				"4p7l9/bpjEN14+30EO2s3MKetZ9Zl6TNF+ic3PIEEMidtbrD3bRtr7jK4LR1" +
				"pvD6DuSn6unaD71GgaSxy5914+30EO2s3OnP566l4Of2GfKe3MKetZ9Zl6TN" +
				"DOul5vvPuIlZl6Sxy59Zl8DyD+NZ6/0BELxbvNO/7uer5vH2zZ+v3PYEFO6n" +
				"tKbC4q1pmaTA6YxDl6Sxy7to2PD9GvZ3hI6xy59Zs/MDD+SrwPL3Gp+d2Pj2" +
				"6KFvprfA3a9qq6axHvSbvPwBFPE=");
			
			var enableLargeAddressSpaceSetting = ConfigurationManager.AppSettings["EOEnableLargeAddressSpace"];
			if (bool.TryParse(enableLargeAddressSpaceSetting, out var enableLargeAddressSpace) && enableLargeAddressSpace)
			{
				EO.Base.Runtime.EnableEOWP = true;
				EO.Base.Runtime.InitWorkerProcessExecutable(System.IO.Path.Combine(Application.StartupPath, "eowp.exe"));
			}

			XmlConfigurator.Configure();

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			// WPF should use CurrentCulture
			FrameworkElement.LanguageProperty.OverrideMetadata(
				typeof(FrameworkElement),
				new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

			if (!createAppConfigReader())
				return;

			setDummyPrincipalBeforeContainerRegistrations();
			ContainerForLegacy.Container = configureContainer();
			TimeZoneGuardForDesktop_DONOTUSE.Set(ContainerForLegacy.Container.Resolve<ITimeZoneGuard>());

			SetReleaseMode();
			 var applicationStarter = ContainerForLegacy.Container.Resolve<ApplicationStartup>();
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
		}

		private static void setDummyPrincipalBeforeContainerRegistrations()
		{
			var teleoptiPrincipal = new TeleoptiPrincipalWithUnsafePerson(new GenericIdentity(""), null as IPerson);
			AppDomain.CurrentDomain.SetThreadPrincipal(teleoptiPrincipal);
			Thread.CurrentPrincipal = teleoptiPrincipal;
		}

		public SmartClientShellApplication(IComponentContext container)
			: base(container)
		{
		}

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

				var iocArgs = new IocArgs(configReader)
				{
					MessageBrokerListeningEnabled = true,
					ImplementationTypeForCurrentUnitOfWork = typeof(FromFactory),
					OptimizeScheduleChangedEvents_DontUseFromWeb = true,
					IsFatClient = true,
					TeleoptiPrincipalForLegacy = true
				};
				var configuration = new IocConfiguration(
					iocArgs,
					CommonModule.ToggleManagerForIoc(iocArgs));

				builder.RegisterModule(new CommonModule(configuration));
				builder.RegisterType<SirLeakAlot>().As<INestedUnitOfWorkStrategy>().SingleInstance();
				builder.RegisterType<WinTenantCredentials>().As<ICurrentTenantCredentials>().SingleInstance();
				builder.RegisterModule(new EncryptionModule(configuration));
				builder.RegisterModule<EventAggregatorModule>();
				builder.RegisterModule(new StartupModule(configuration));
				builder.RegisterModule(new NavigationModule(configuration));
				builder.RegisterModule(new BudgetModule(configuration));
				builder.RegisterModule<IntradayModule>();
				builder.RegisterModule(new ForecasterModule(configuration));
				builder.RegisterModule(new ScheduleScreenRefresherModule(configuration));
				builder.RegisterModule<MeetingOverviewModule>();
				builder.RegisterModule<SchedulingServiceModule>();
				builder.RegisterModule<ShiftsModule>();
				builder.RegisterModule(new PersonSelectorModule(configuration));
				builder.RegisterModule<RequestHistoryModule>();
				builder.RegisterModule<MainModule>();
				builder.RegisterModule(new OutboundScheduledResourcesProviderModule());
				//hack to get old behavior work
				builder.Register(context => context.Resolve<ICurrentUnitOfWorkFactory>().Current()).ExternallyOwned().As<IUnitOfWorkFactory>();
				builder.RegisterType<CurrentUnitOfWorkFactory>().As<ICurrentUnitOfWorkFactory>().SingleInstance();
				//////
				builder.RegisterType<RtaConfigurationValidationPoller>().SingleInstance();

				builder.Register(c => new WebConfigReader(() =>
				{
					var webSettings = new WebSettings
					{
						Settings =
							c.Resolve<ISharedSettingsTenantClient>()
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
			var fallBack = string.Empty;

			var toggles = exceptionSafeTogglesActive();
			var exceptionMessageBuilder = new ExceptionMessageBuilder(ex, toggles);

			var log = LogManager.GetLogger(typeof(SmartClientShellApplication));
			log.Error(exceptionMessageBuilder.BuildSimpleExceptionMessage(), ex);

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

		private static ITogglesActive exceptionSafeTogglesActive()
		{
			ITogglesActive toggles;
			try
			{
				var iocArgs = new IocArgs(configReader)
				{
					TeleoptiPrincipalForLegacy = true
				};
				var tempContainerBecauseWeDontHaveAGlobalOneHere = new ContainerBuilder();
				tempContainerBecauseWeDontHaveAGlobalOneHere.RegisterModule(
					new CommonModule(new IocConfiguration(iocArgs, CommonModule.ToggleManagerForIoc(iocArgs))));
				using (var container = tempContainerBecauseWeDontHaveAGlobalOneHere.Build())
				{
					toggles = container.Resolve<ITogglesActive>();
				}
			}
			catch (Exception)
			{
				toggles = new emptyStubOfActivteToggles_OnlyUseIfWebCannotBeReached();
			}

			return toggles;
		}

		private class emptyStubOfActivteToggles_OnlyUseIfWebCannotBeReached : ITogglesActive
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
				baseRibbonFormToClose?.FormKill();
			}
		}
	}
}