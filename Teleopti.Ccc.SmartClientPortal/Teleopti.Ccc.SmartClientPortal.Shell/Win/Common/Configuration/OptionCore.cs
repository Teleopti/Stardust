using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Payroll.DefinitionSets;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Payroll.Overtime;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration.Rta;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
	public sealed class OptionCore : IDisposable
	{
		public OptionCore(ISettingPagesProvider settingPagesProvider)
		{
			AllSelectedPages = new List<ISettingPage>();
			AllSupportedPages = settingPagesProvider.CreateSettingPages();
		}

		public ICollection<ISettingPage> AllSupportedPages { get; private set; }
		public ICollection<ISettingPage> AllSelectedPages { get; private set; }
		public IUnitOfWork UnitOfWork { get; private set; }
		public ISettingPage LastPage { get; private set; }

		public bool HasLastPage
		{
			get { return LastPage != null; }
		}

		public void SetUnitOfWork(IUnitOfWork unitOfWork)
		{
			AllSelectedPages.Clear();
			UnitOfWork = unitOfWork;
		}

		public bool MarkAsSelected(ISettingPage page, SelectedEntity<IAggregateRoot> item)
		{
			if (UnitOfWork == null)
				throw new InvalidOperationException(
					"The unit of work must be set through SetUnitOfWork before this operation is called");

			var pageSupported = AllSupportedPages.Contains(page);
			if (pageSupported) LastPage = page;

			var pageNotSelected = !AllSelectedPages.Contains(page);
			if (pageSupported && pageNotSelected)
			{
				AllSelectedPages.Add(page);
				page.InitializeDialogControl();
				page.SetUnitOfWork(UnitOfWork);
				if (item == null)
					page.LoadControl();
			}
			if (pageSupported && item != null)
			{
				page.LoadFromExternalModule(item);
			}
			return pageSupported;
		}

		public bool MarkAsSelected(Type pageType, SelectedEntity<IAggregateRoot> item)
		{
			var page = AllSupportedPages.FirstOrDefault(p => p.GetType() == pageType);
			var ready = page != null;
			if (ready) ready = MarkAsSelected(page, item);
			return ready;
		}

		public void SaveChanges()
		{
			var invalidModules = new StringBuilder();

			foreach (var page in AllSelectedPages)
			{
				try
				{
					page.SaveChanges();
				}
				catch (ValidationException ex)
				{
					invalidModules.Append(ex.Message + ", ");
				}
			}
			if (invalidModules.Length > 0)
			{
				throw new ValidationException(invalidModules.ToString().Substring(0, invalidModules.Length - 2));
			}

			using (var runSql = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				UnitOfWork.PersistAll();
				runSql.PersistAll();
			}
		}

		public void UnloadPages()
		{
			foreach (var page in AllSelectedPages)
			{
				page.Unload();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				ReleaseManagedResources();
			}
			ReleaseUnmanagedResources();
		}

		private void ReleaseManagedResources()
		{
			AllSelectedPages.Clear();
			foreach (IDisposable page in AllSupportedPages)
			{
				((ISettingPage)page).Unload();
				page.Dispose();
			}
			AllSupportedPages.Clear();
		}

		private static void ReleaseUnmanagedResources()
		{
		}
	}

	public interface ISettingPagesProvider
	{
		IList<ISettingPage> CreateSettingPages();
	}

	public class OptionsSettingPagesProvider : ISettingPagesProvider
	{
		private readonly IToggleManager _toggleManager;
		private readonly IBusinessRuleConfigProvider _businessRuleConfigProvider;

		public OptionsSettingPagesProvider(IToggleManager toggleManager, IBusinessRuleConfigProvider businessRuleConfigProvider)
		{
			_toggleManager = toggleManager;
			_businessRuleConfigProvider = businessRuleConfigProvider;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public IList<ISettingPage> CreateSettingPages()
		{
			var allSupportedPages = new List<ISettingPage>
			{
				new KpiSettings(),
				new ScorecardSettings(),
				new SetScorecardView(),
				new ContractControl(_toggleManager),
				new ContractScheduleControl(),
				new PartTimePercentageControl(),
				new ActivityControl(),
				new MasterActivitiesControl(),
				new AbsenceControl(),
				new ScenarioPage(),
				new CommonAgentNameDescriptionControl(CommonAgentNameDescriptionType.Common),
				new CommonAgentNameDescriptionControl(CommonAgentNameDescriptionType.ScheduleExport),
				new OptionalColumnsControl(_toggleManager),
				new OrganizationTreeControl(),
				new SiteControl(),
				new StateGroupControl(),
				new ShiftCategorySettingsControl(),
				new AvailabilityPage(),
				new SystemSettingControl(_toggleManager),
				new RotationPage(),
				new DaysOffControl(),
				new AlarmControl(alarmControlPresenterDecorators()),
				new ManageAlarmSituations(),
				new DefinitionSetSettings(),
				new WorkflowControlSetView(_toggleManager),
				new AuditingPage(),
				new ScheduleTagControl(),
				new GamificationSettingControl(_toggleManager),
				new SetGamificationSettingTargetsControl(),
				new SeniorityControl()
			};

			if (PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.PayrollIntegration))
				allSupportedPages.Add(new MultiplicatorControlView());

			if (DefinedLicenseDataFactory.GetLicenseActivator(UnitOfWorkFactory.Current.Name)
				.EnabledLicenseOptionPaths.Contains(DefinedLicenseOptionPaths.TeleoptiCccSmsLink))
			{
				if (!_toggleManager.IsEnabled(Toggles.Settings_AlertViaEmailFromSMSLink_30444))
					allSupportedPages.Add(new SmsSettingsControl());
				else
					allSupportedPages.Add(new NotificationSettingsControl(_toggleManager));
			}


			if (_toggleManager.IsEnabled (Toggles.Wfm_Requests_Check_Max_Seats_39937))
			{
				allSupportedPages.Add (new ShiftTradeSystemSettings(_toggleManager, _businessRuleConfigProvider));
			}

			return allSupportedPages;
		}

		private IEnumerable<IAlarmControlPresenterDecorator> alarmControlPresenterDecorators()
		{
			yield return new AdherenceColumn();
			yield return new ThresholdColumn();
			yield return new IsAlarmColumn();
			yield return new AlarmColorColumn();
		}
	}

	public class PeopleSettingPagesProvider : ISettingPagesProvider
	{
		private readonly IToggleManager _toggleManager;

		public PeopleSettingPagesProvider(IToggleManager toggleManager)
		{
			_toggleManager = toggleManager;
		}

		public IList<ISettingPage> CreateSettingPages()
		{
			var allSupportedPages = new List<ISettingPage>
			{
				new ContractControl(_toggleManager),
				new ContractScheduleControl(),
				new PartTimePercentageControl()
			};
			return allSupportedPages;
		}
	}

	public class MyProfileSettingPagesProvider : ISettingPagesProvider
	{
		private readonly IComponentContext _container;

		public MyProfileSettingPagesProvider(IComponentContext container)
		{
			_container = container;
		}

		public IList<ISettingPage> CreateSettingPages()
		{
			var allSupportedPages = new List<ISettingPage> { new ChangePasswordControl(_container) };
			return allSupportedPages;
		}
	}
}