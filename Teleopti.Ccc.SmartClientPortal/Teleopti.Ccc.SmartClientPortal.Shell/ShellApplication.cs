using System;
using System.Collections;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Markup;
using System.Xaml;
using Autofac;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Win.Forecasting;
using log4net.Config;
using MbCache.Configuration;
using Microsoft.Practices.CompositeUI;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.SmartClientPortal.Common.Constants;
using Teleopti.Ccc.SmartClientPortal.Common.Library;
using Teleopti.Ccc.Win;
using Teleopti.Ccc.Win.Budgeting;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.ExceptionHandling;
using Teleopti.Ccc.Win.Forecasting.Forms.QuickForecast;
using Teleopti.Ccc.Win.Grouping;
using Teleopti.Ccc.Win.Intraday;
using Teleopti.Ccc.Win.Main;
using Teleopti.Ccc.Win.Meetings.Overview;
using Teleopti.Ccc.Win.Permissions;
using Teleopti.Ccc.Win.Scheduling;
using Teleopti.Ccc.Win.Shifts;
using Teleopti.Ccc.WinCode.Common.ExceptionHandling;
using Teleopti.Ccc.WinCode.Events;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Application=System.Windows.Forms.Application;
using MessageBox=System.Windows.Forms.MessageBox;

namespace Teleopti.Ccc.SmartClientPortal.Shell
{
    /// <summary>
    /// Main application entry point class.
    /// Note that the class derives from CAB supplied base class FormSmartClientShellApplication, and the 
    /// main form will be SmartClientShellForm, also created by default by this solution template
    /// </summary>
    class SmartClientShellApplication : SmartClientApplication<WorkItem, SmartClientShellForm>
    {
        /// <summary>
        /// Shell application entry point.
        /// </summary>
        [STAThread]
        static void Main()
        {
            XmlConfigurator.Configure();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // WPF should use CurrentCulture
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            IContainer container = configureContainer();
#if (!DEBUG)
            //NHibernateProfiler.Initialize();
            SetReleaseMode();
           if (CheckFrameWorkVersion())
            {
                ApplicationStartup applicationStarter = container.Resolve<ApplicationStartup>();
                if (applicationStarter.LogOn())
                {
                    try
                    {
                        applicationStarter.LoadShellApplication();
                    }
                    catch (Exception exception)
                    {
                        HandleException(exception);
                    }
                }
            }
#endif
#if (DEBUG)
            if (CheckFrameWorkVersion())
            {
                ApplicationStartup applicationStarter = container.Resolve<ApplicationStartup>();
                if (applicationStarter.LogOn())
                {
                	killNotNeededSessionFactories();
                    applicationStarter.LoadShellApplication();
                }
            }
#endif
        }

    	private static void killNotNeededSessionFactories()
    	{
			//kan man inte f� tag p� datasource p� n�t l�ttare s�tt?
    		var loggedOnDataSource = ((ITeleoptiIdentity) TeleoptiPrincipal.Current.Identity).DataSource;
    		StateHolder.Instance.StateReader.ApplicationScopeData.DisposeAllDataSourcesExcept(loggedOnDataSource);
    	}

    	public SmartClientShellApplication(IComponentContext container) : base(container)
        {
        }

        private static IContainer configureContainer()
        {
            using (PerformanceOutput.ForOperation("Building Ioc container"))
            {
                var builder = new ContainerBuilder();

					 var mbCacheModule = new MbCacheModule(null);
				builder.RegisterModule(mbCacheModule);
				builder.RegisterModule(new RuleSetModule(mbCacheModule, true));
				builder.RegisterModule(new EncryptionModule());
                builder.RegisterModule(new AuthenticationModule());
                builder.RegisterModule(new EventAggregatorModule());
                builder.RegisterModule(new UnitOfWorkModule());
                builder.RegisterModule(new RepositoryModule());
                builder.RegisterModule(new StartupModule());
                builder.RegisterModule(new NavigationModule());
                builder.RegisterModule(new BudgetModule());
                builder.RegisterModule<IntradayModule>();
                builder.RegisterModule<ForecasterModule>();
                builder.RegisterModule(new ScheduleScreenPersisterModule());
                builder.RegisterModule(new ScheduleScreenRefresherModule());
                builder.RegisterModule(new MeetingOverviewModule());
                builder.RegisterModule(new SchedulingServiceModule());
                builder.RegisterModule(new ShiftsModule());
                builder.RegisterModule(new PersonSelectorModule());
                builder.RegisterModule(new PermissionsModule());
                builder.RegisterModule(new RequestHistoryModule());
							//hack to get old behavior work
	            builder.Register(context => context.Resolve<ICurrentUnitOfWorkFactory>().LoggedOnUnitOfWorkFactory());
                return builder.Build();
            }

            
        }

        /// <summary>
        /// Runs the application in release mode.
        /// </summary>
        private static void SetReleaseMode()
        {
            AppDomain.CurrentDomain.UnhandledException += AppDomainUnhandledException;
            Application.ThreadException += ApplicationThreadException;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private static bool CheckFrameWorkVersion()
        {
            bool found = Detector.Found35SP1();
            if (!found)
                MessageBox.Show(
                    ".Net Framework 3.5 SP1 could not be detected. \r\n TeleoptiCCC version 7 requires that .Net Framework 3.5 SP1 is installed",
                    "TeleoptiCCC version 7 can not start", MessageBoxButtons.OK, MessageBoxIcon.Error);

            return found;
        }

        protected override void BeforeShellCreated()
        {
            base.BeforeShellCreated();
        }

        /// <summary>
        /// Sets the extension site registration after the SmartClientShell has been created.
        /// </summary>
        protected override void AfterShellCreated()
        {
            base.AfterShellCreated();
            //never used
            //RootWorkItem.UIExtensionSites.RegisterSite(UIExtensionSiteNames.MainMenu, Shell.MainMenuStrip);
            RootWorkItem.UIExtensionSites.RegisterSite(UIExtensionSiteNames.MainStatus, Shell.MainStatusStrip);
        }

        /// <summary>
        /// Handles the unhandled exception event in the application.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.UnhandledExceptionEventArgs"/> instance containing the event data.</param>
        private static void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception);
        }

        private static void ApplicationThreadException(object sender, ThreadExceptionEventArgs e)
        {
            HandleException(e.Exception);
        }

        /// <summary>
        /// Handles the exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static void HandleException(Exception ex)
        {
            if (ex == null) return;

            Application.ThreadException -= ApplicationThreadException;
            AppDomain.CurrentDomain.UnhandledException -= AppDomainUnhandledException;
            string fallBack = string.Empty;

            StringSetting emailSetting = new StringSetting();
            if (StateHolderReader.IsInitialized)
            {
                try
                {
                    using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                    {
                        ISettingDataRepository settingDataRepository = new GlobalSettingDataRepository(uow);
                        emailSetting = settingDataRepository.FindValueByKey("SupportEmailSetting",
                                                                            new StringSetting());
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
            IWriteToFile fileWriter = new WriteStringToFile();
            IMapiMailMessage message = new MapiMailMessage(string.Empty, string.Empty);
            ExceptionHandlerModel exceptionHandlerModel = new ExceptionHandlerModel(ex, defaultEmail,
                                                                                    message, fileWriter);
            using (var view = new ExceptionHandlerView(exceptionHandlerModel))
            {
                view.ShowDialog();
            }
            KillOpenForms();
            Application.Exit();

        }

        private static void KillOpenForms()
        {
            ArrayList forms = new ArrayList(Application.OpenForms);
            for (int i = 0; i < forms.Count; i++)
            {
                BaseRibbonForm baseRibbonFormToClose =forms[i] as BaseRibbonForm;
                if (baseRibbonFormToClose != null)
                {
                    baseRibbonFormToClose.FormKill();
                }
            }
        }
    }
}
