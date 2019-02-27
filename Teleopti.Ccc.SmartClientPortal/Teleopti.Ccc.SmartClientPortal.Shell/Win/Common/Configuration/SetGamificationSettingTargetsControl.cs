using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
	public partial class SetGamificationSettingTargetsControl : BaseUserControl, ISettingPage, ISetGamificationSettingView
	{
		private readonly IList<TeamGamificationSettingModel> _source = new List<TeamGamificationSettingModel>();
		private SFGridColumnGridHelper<TeamGamificationSettingModel> _columnGridHelper;
		private SetGamificationSettingPresenter _presenter;
		private IList<IGamificationSetting> _settings = new List<IGamificationSetting>();
		

		public SetGamificationSettingTargetsControl()
		{
			InitializeComponent();

			gridControlSelectSettingForTeams.DefaultRowHeight = 18;
			gridControlSelectSettingForTeams.MinResizeRowSize = 18;

			var currentUow = new CurrentUnitOfWork(CurrentUnitOfWorkFactory.Make());
			_presenter = new SetGamificationSettingPresenter(this, UnitOfWorkFactory.Current,
															new TeamGamificationSettingRepository(currentUow),
															new SiteProvider(SiteRepository.DONT_USE_CTOR(currentUow)),
															new TeamProvider(new TeamRepository(currentUow)),
															new GamificationSettingProvider(new GamificationSettingRepository(currentUow))
															);
		}

		private void comboBoxSiteSelectedIndexChanged(object sender, EventArgs e)
		{
			if ((ISite)comboBoxSite.SelectedItem != null)
			{
				_presenter.UpdateTeamGamificationSettings(_source);
			}
			_presenter.SelectSite((ISite) comboBoxSite.SelectedItem);
			
		}

		private void gridControlSelectSettingForTeamsKeyUp(object sender, KeyEventArgs e)
		{
			gridControlSelectSettingForTeams.Refresh();
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

			tableLayoutPanelSubHeader2.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader2.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader2.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
		}

		public void Unload()
		{
		}

		private ReadOnlyCollection<SFGridColumnBase<TeamGamificationSettingModel>> configureGrid()
		{
			IList<SFGridColumnBase<TeamGamificationSettingModel>> gridColumns = new List<SFGridColumnBase<TeamGamificationSettingModel>>();

			gridControlSelectSettingForTeams.Rows.HeaderCount = 0;
			// Grid must have a Header column
			gridColumns.Add(new SFGridRowHeaderColumn<TeamGamificationSettingModel>(string.Empty));

			gridColumns.Add(new SFGridDescriptionNameColumn<TeamGamificationSettingModel>("SiteAndTeam", Resources.Team));

			var gamificationSettingDropdownColumn  = new SFGridDropDownColumn<TeamGamificationSettingModel, IGamificationSetting>("GamificationSetting", Resources.Setting,
																			  _settings, "Description",
																			  typeof(IGamificationSetting));
			gridColumns.Add(gamificationSettingDropdownColumn);
			gamificationSettingDropdownColumn.QueryComboItems += gamificationSettingDropdownColumnQueryComboItems;

			gridControlSelectSettingForTeams.RowCount = gridRowCount();
			gridControlSelectSettingForTeams.ColCount = gridColumns.Count - 1;  //col index starts on 0
			return new ReadOnlyCollection<SFGridColumnBase<TeamGamificationSettingModel>>(gridColumns);
		}

		private void gamificationSettingDropdownColumnQueryComboItems(object sender, GridQueryCellInfoEventArgs e)
		{
			e.Style.DataSource = _settings;
		}

		private int gridRowCount()
		{
			return _source.Count + gridControlSelectSettingForTeams.Rows.HeaderCount;
		}

		public void SaveChanges()
		{
			_presenter.UpdateTeamGamificationSettings(_source);
			Persist();
			_presenter.SelectSite((ISite)comboBoxSite.SelectedItem);
		}

		public TreeFamily TreeFamily()
		{
			return new TreeFamily(Resources.Gamification);
		}

		public string TreeNode()
		{
			return Resources.SetSettingTargets;
		}
		
		public ViewType ViewType
		{
			get { return ViewType.Gamification; }
		}

		public void LoadControl()
		{
			
			_presenter.Initialize();
		}
		public void OnShow()
		{}

		public void SetUnitOfWork(IUnitOfWork value)
		{}

		public void Persist()
		{
			_presenter.SaveChanges();
		}

		public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
		{}

		public void SetSites(IEnumerable<ISite> sites)
		{
			comboBoxSite.SelectedIndexChanged -= comboBoxSiteSelectedIndexChanged;

			comboBoxSite.Items.Clear();
			comboBoxSite.Sorted = false;
			comboBoxSite.DisplayMember = "Description";
			comboBoxSite.ValueMember = "Id";

			foreach (ISite item in sites)
			{
				comboBoxSite.Items.Add(item);
			}

			comboBoxSite.SelectedIndexChanged += comboBoxSiteSelectedIndexChanged;
		}

		public void SetGamificationSettings(IEnumerable<IGamificationSetting> gamificationSettings)
		{
			_settings.Clear();
			gamificationSettings.ForEach(s => _settings.Add(s));
		}

		public void SetSelectedSite(ISite site)
		{
			comboBoxSite.SelectedIndexChanged -= comboBoxSiteSelectedIndexChanged;
			comboBoxSite.SelectedIndex = -1;
			comboBoxSite.SelectedIndexChanged += comboBoxSiteSelectedIndexChanged;
			comboBoxSite.SelectedItem = site;
		}

		public void SetTeams(IList<TeamGamificationSettingModel> models)
		{
			IEnumerable<TeamGamificationSettingModel> query = models.OrderBy(t => t.SiteAndTeam);

			_source.Clear();
			foreach (TeamGamificationSettingModel target in query)
			{
				_source.Add(target);
			}

			ReadOnlyCollection<SFGridColumnBase<TeamGamificationSettingModel>> x = configureGrid();
			_columnGridHelper = new SFGridColumnGridHelper<TeamGamificationSettingModel>(gridControlSelectSettingForTeams, x,
				_source, false)
			{AllowExtendedCopyPaste = true};
		}
	}
}
