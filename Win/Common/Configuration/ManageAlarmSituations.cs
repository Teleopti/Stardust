using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Foundation;
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
		private readonly IRtaControlNamer _mapNamer;
		private ManageAlarmSituationView _view;


		public ManageAlarmSituations(IRtaControlNamer mapNamer)
		{
			_mapNamer = mapNamer;
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
			labelHeader.Text = _mapNamer.Title();
			labelSubHeader1.Text = _mapNamer.PanelHeader();
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
																new RtaRuleRepository(currentUnitOfWork),
																new RtaStateGroupRepository(currentUnitOfWork),
																new ActivityRepository(currentUnitOfWork),
																new RtaMapRepository(currentUnitOfWork),
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
			return _mapNamer.TreeNodeName();
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
