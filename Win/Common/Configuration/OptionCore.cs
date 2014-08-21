using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.Payroll.DefinitionSets;
using Teleopti.Ccc.Win.Payroll.Overtime;
using Teleopti.Ccc.Win.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Common.Configuration
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
			if (UnitOfWork == null) throw new InvalidOperationException("The unit of work must be set through SetUnitOfWork before this operation is called");

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
				else
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

		public OptionsSettingPagesProvider(IToggleManager toggleManager)
		{
			_toggleManager = toggleManager;
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
											new OptionalColumnsControl(),
											new OrganizationTreeControl(),
											new SiteControl(),
											new StateGroupControl(),
											new ShiftCategorySettingsControl(),
											new AvailabilityPage(),
											new SystemSettingControl(),
											new RotationPage(),
											new DaysOffControl(),
											new AlarmControl(),
											new ManageAlarmSituations(),
											new FairnessValuesControl(),
											new DefinitionSetSettings(),
											new WorkflowControlSetView(_toggleManager),
											new AuditingPage(),
											new ScheduleTagControl()
										};

			if (_toggleManager.IsEnabled(Toggles.MyTimeWeb_AgentBadge_28913))
			{
				allSupportedPages.Add(new BadgeThresholdSettings());
			}

			if (PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.PayrollIntegration))
				allSupportedPages.Add(new MultiplicatorControlView());
			
			if (DefinedLicenseDataFactory.GetLicenseActivator(UnitOfWorkFactory.Current.Name).EnabledLicenseOptionPaths.Contains(DefinedLicenseOptionPaths.TeleoptiCccSmsLink))
				allSupportedPages.Add(new SmsSettingsControl());

			return allSupportedPages;
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
		public IList<ISettingPage> CreateSettingPages()
		{
			var allSupportedPages = new List<ISettingPage> { new ChangePasswordControl() };
			return allSupportedPages;
		}
	}
}