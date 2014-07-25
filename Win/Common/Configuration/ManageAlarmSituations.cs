using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common.Configuration;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Common.Configuration
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

		private void setColors()
		{
			BackColor = ColorHelper.WizardBackgroundColor();
			tableLayoutPanelBody.BackColor = ColorHelper.WizardBackgroundColor();

			gradientPanelHeader.BackColor = ColorHelper.OptionsDialogHeaderBackColor();
			labelHeader.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();

			tableLayoutPanelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader1.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();

			teleoptiGridControl1.BackColor = ColorHelper.GridControlGridInteriorColor();
			teleoptiGridControl1.Properties.BackgroundColor = ColorHelper.WizardBackgroundColor();
		}

		public void LoadControl()
		{
			_view = new ManageAlarmSituationView(teleoptiGridControl1);
			var currentUnitOfWork = UnitOfWorkFactory.CurrentUnitOfWork();
			_view.Presenter = new ManageAlarmSituationPresenter(UnitOfWorkFactory.Current,
																new AlarmTypeRepository(currentUnitOfWork),
																new RtaStateGroupRepository(currentUnitOfWork),
																new ActivityRepository(currentUnitOfWork),
																new StateGroupActivityAlarmRepository(currentUnitOfWork),
																StateHolderReader.Instance.StateReader.ApplicationScopeData
																				 .Messaging, _view);
			_view.LoadGrid();
		}

		public TreeFamily TreeFamily()
		{
			return new TreeFamily(Resources.RealTimeAdherence, DefinedRaptorApplicationFunctionPaths.ManageRealTimeAdherence);
		}

		public string TreeNode()
		{
			return Resources.Alarms;
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
