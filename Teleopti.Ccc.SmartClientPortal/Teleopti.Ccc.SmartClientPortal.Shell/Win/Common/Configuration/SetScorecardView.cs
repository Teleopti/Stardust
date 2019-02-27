using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public partial class SetScorecardView : BaseUserControl, ISettingPage, ISetScorecardView
	{
		private readonly IList<ITeamScorecardModel> _source = new List<ITeamScorecardModel>();
		private SFGridColumnGridHelper<ITeamScorecardModel> _columnGridHelper;
		private IUnitOfWork _unitOfWork;
		private SetScorecardPresenter _presenter;
		private IList<IScorecard> _scorecards;

		public SetScorecardView()
		{
			InitializeComponent();

			gridControl1.KeyUp += gridControl1KeyUp;
			gridControl1.DefaultRowHeight = 18;
			gridControl1.MinResizeRowSize = 18;
		}

		private void comboBoxSiteSelectedIndexChanged(object sender, EventArgs e)
		{
			_presenter.SelectSite((ISite) comboBoxSite.SelectedItem);
		}

		private void gridControl1KeyUp(object sender, KeyEventArgs e)
		{
			gridControl1.Refresh();
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

			gridControl1.BackColor = ColorHelper.GridControlGridInteriorColor();
			gridControl1.Properties.BackgroundColor = ColorHelper.WizardBackgroundColor();
		}

		public void Unload()
		{
			if (_presenter != null)
			{
				_presenter.Dispose();
			}
		}

		public void LoadControl()
		{
			_presenter = new SetScorecardPresenter(this, _unitOfWork, MessageBrokerInStateHolder.Instance,
												   new ScorecardProvider(new ScorecardRepository(_unitOfWork), true),
												   new SiteProvider(SiteRepository.DONT_USE_CTOR(_unitOfWork)),
												   new TeamProvider(new TeamRepository(_unitOfWork)));

			_presenter.Initialize();
		}

		public void SaveChanges()
		{
		}

		public TreeFamily TreeFamily()
		{
			return new TreeFamily(UserTexts.Resources.Scorecards, DefinedRaptorApplicationFunctionPaths.ManageScorecards);
		}

		public string TreeNode()
		{
			return UserTexts.Resources.SetScorecard;
		}

		public void OnShow()
		{
		}

		public void SetUnitOfWork(IUnitOfWork value)
		{
			_unitOfWork = value;
		}

		public void Persist()
		{
		}

		private ReadOnlyCollection<SFGridColumnBase<ITeamScorecardModel>> configureGrid()
		{
			IList<SFGridColumnBase<ITeamScorecardModel>> gridColumns = new List<SFGridColumnBase<ITeamScorecardModel>>();

			gridControl1.Rows.HeaderCount = 0;
			// Grid must have a Header column
			gridColumns.Add(new SFGridRowHeaderColumn<ITeamScorecardModel>(string.Empty));

			gridColumns.Add(new SFGridDescriptionNameColumn<ITeamScorecardModel>("SiteAndTeam", UserTexts.Resources.Team));
			var scorecardColumn = new SFGridDropDownColumn<ITeamScorecardModel, IScorecard>("Scorecard", UserTexts.Resources.ScoreCard,
																			  _scorecards, "Name",
																			  typeof(IScorecard));
			gridColumns.Add(scorecardColumn);
			scorecardColumn.QueryComboItems += scorecardColumnQueryComboItems;
			
			gridControl1.RowCount = gridRowCount();
			gridControl1.ColCount = gridColumns.Count - 1;  //col index starts on 0
			return new ReadOnlyCollection<SFGridColumnBase<ITeamScorecardModel>>(gridColumns);
		}

		private void scorecardColumnQueryComboItems(object sender, GridQueryCellInfoEventArgs e)
		{
			e.Style.DataSource = _scorecards;
		}

		private int gridRowCount()
		{
			return _source.Count + gridControl1.Rows.HeaderCount;
		}

		public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
		{
		}

		public ViewType ViewType
		{
			get { return ViewType.SetScorecard; }
		}

		private void setScorecardLayout(object sender, LayoutEventArgs e)
		{
			gridControl1.ColWidths.ResizeToFit(GridRangeInfo.Cols(1, 2), GridResizeToFitOptions.IncludeHeaders);
		}

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

		public void SetScorecards(IEnumerable<IScorecard> scorecards)
		{
			_scorecards = new List<IScorecard>(scorecards);
		}

		public void SetSelectedSite(ISite site)
		{
			comboBoxSite.SelectedIndexChanged -= comboBoxSiteSelectedIndexChanged;
			comboBoxSite.SelectedIndex = -1;
			comboBoxSite.SelectedIndexChanged += comboBoxSiteSelectedIndexChanged;
			comboBoxSite.SelectedItem = site;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public void SetTeams(IList<ITeamScorecardModel> models)
		{
			IEnumerable<ITeamScorecardModel> query = models.OrderBy(t => t.SiteAndTeam);

			_source.Clear();
			foreach (ITeamScorecardModel target in query)
			{
				_source.Add(target);
			}

			ReadOnlyCollection<SFGridColumnBase<ITeamScorecardModel>> x = configureGrid();
			_columnGridHelper = new SFGridColumnGridHelper<ITeamScorecardModel>(gridControl1, x, _source, false)
									{AllowExtendedCopyPaste = true};
		}
	}
}