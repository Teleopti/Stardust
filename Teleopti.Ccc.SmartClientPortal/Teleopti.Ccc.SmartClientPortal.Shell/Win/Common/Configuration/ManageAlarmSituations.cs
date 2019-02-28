using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.UserTexts;
using Teleopti.Wfm.Adherence.Configuration.Repositories;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
	public partial class ManageAlarmSituations : BaseUserControl, ISettingPage
	{
		private ManageAlarmSituationView _view;
		
		public ManageAlarmSituations()
		{
			InitializeComponent();
		}

		public void SaveChanges()
		{
			_view.Presenter.OnSave();
		}

		public void Unload()
		{
			//think everything is handled in dispose
		}

		public void SetUnitOfWork(IUnitOfWork value)
		{
		}

		public void Persist()
		{
			
		}

		public void InitializeDialogControl()
		{
			setColors();
			SetTexts();
		}

		protected override void SetCommonTexts()
		{
			base.SetCommonTexts();
			labelHeader.Text = Resources.ManageMappings;
			labelSubHeader1.Text = Resources.Mappings;
		}

		private void setColors()
		{
			BackColor = ColorHelper.WizardBackgroundColor();
			tableLayoutPanelBody.BackColor = ColorHelper.WizardBackgroundColor();

			gradientPanelHeader.BackColor = ColorHelper.OptionsDialogHeaderBackColor();
			labelHeader.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();

			tableLayoutPanelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader1.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();

			gridControlAlarms.BackColor = ColorHelper.GridControlGridInteriorColor();
			gridControlAlarms.Properties.BackgroundColor = ColorHelper.WizardBackgroundColor();
		}

		public void LoadControl()
		{
			_view = new ManageAlarmSituationView(gridControlAlarms);
			var currentUnitOfWork = UnitOfWorkFactory.CurrentUnitOfWork();
			_view.Presenter = new ManageAlarmSituationPresenter(UnitOfWorkFactory.Current,
																RtaRuleRepository.DONT_USE_CTOR(currentUnitOfWork),
																RtaStateGroupRepository.DONT_USE_CTOR(currentUnitOfWork),
																ActivityRepository.DONT_USE_CTOR(currentUnitOfWork, null, null),
																RtaMapRepository.DONT_USE_CTOR(currentUnitOfWork),
																MessageBrokerInStateHolder.Instance, 
																_view);
			_view.LoadGrid();
		}

		public TreeFamily TreeFamily()
		{
			return new TreeFamily(Resources.RealTimeAdherence, DefinedRaptorApplicationFunctionPaths.ManageRealTimeAdherence);
		}

		public string TreeNode()
		{
			return Resources.Mappings;
		}

		public void OnShow()
		{
		}


		public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
		{
			throw new System.NotImplementedException();
		}

		public ViewType ViewType
		{
			get { return ViewType.ManageAlarmSituations; }
		}
	}
}
