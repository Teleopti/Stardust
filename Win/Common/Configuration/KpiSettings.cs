﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Kpi;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Win.Common.Configuration.Columns;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Common.Configuration
{

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public partial class KpiSettings : BaseUserControl, ISettingPage
    {
        private IList<IKpiTarget> _lstTargets;
        private IList<ITeam> _lstTeams;
        private IKeyPerformanceIndicator _selectedKpi;
        private readonly IList<IKpiTarget> _source = new List<IKpiTarget>();
        private SFGridColumnGridHelper<IKpiTarget> _columnGridHelper;
        private IUnitOfWork _unitOfWork;

        private IList<IKpiTarget> KpiTargets
        {
            get
            {
                if (_lstTargets == null)
                {
                    var tarRep = new KpiTargetRepository(_unitOfWork);
                    _lstTargets = tarRep.LoadAll();
                }
                return _lstTargets;
            }
        }

        private IEnumerable<ITeam> Teams
        {
            get
            {
                if (_lstTeams == null)
                {
                    var teamRep = new TeamRepository(_unitOfWork);
                    _lstTeams = teamRep.LoadAll();
                }
                return _lstTeams;
            }
        }

        private IKeyPerformanceIndicator SelectedKpi
        {
            get
            {
                return _selectedKpi;
            }
        }

        public KpiSettings()
        {
            InitializeComponent();

            comboBoxKpi.SelectedIndexChanged += ComboBoxKpiSelectedIndexChanged;
            comboBoxSite.SelectedIndexChanged += ComboBoxSiteSelectedIndexChanged;

            gridControl1.KeyUp += GridControl1KeyUp;
            gridControl1.DefaultRowHeight = 18;
            gridControl1.MinResizeRowSize = 18;
        }

        private void ComboBoxSiteSelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTeam();
        }

        private void GridControl1KeyUp(object sender, KeyEventArgs e)
        {
            gridControl1.Refresh();
        }

        private void KPISettings_Load(object sender, EventArgs e)
        {
            addCellModels();
        }

        private void ComboBoxKpiSelectedIndexChanged(object sender, EventArgs e)
        {
            HandleSelectKpi();
        }

        public void InitializeDialogControl()
        {
            SetColors();
            SetTexts();
        }

        private void SetColors()
        {
            BackColor = ColorHelper.WizardBackgroundColor();
            tableLayoutPanelBody.BackColor = ColorHelper.WizardBackgroundColor();

            gradientPanelHeader.BackgroundColor = ColorHelper.OptionsDialogHeaderGradientBrush();
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
        }

        public void LoadControl()
        {
            LoadSites();
            LoadKpi();
        }

        public void SaveChanges()
        {}

        public TreeFamily TreeFamily()
        {
            return new TreeFamily(UserTexts.Resources.Scorecards, DefinedRaptorApplicationFunctionPaths.ManageScorecards);
        }

        public string TreeNode()
        {
            return UserTexts.Resources.SetKPITargets;
        }

    	public void OnShow()
    	{
    	}

    	public void SetUnitOfWork(IUnitOfWork value)
        {
            _unitOfWork = value;
        }

        public void Persist()
        {}

        private void HandleSelectKpi()
        {
            if (comboBoxKpi.SelectedValue == null)
            {
                return;
            }
            var kpiRep = new KpiRepository(_unitOfWork);
            _selectedKpi = kpiRep.Load((Guid)comboBoxKpi.SelectedValue);

            LoadTeam();
        }

        private void LoadKpi()
        {
            if (comboBoxKpi.Items.Count > 0) return;

            var kpiRep = new KpiRepository(_unitOfWork);
            IList<IKeyPerformanceIndicator> lst = kpiRep.LoadAll();
            var collection = new TypedBindingCollection<IKeyPerformanceIndicator>();
            lst.ForEach(collection.Add);

            comboBoxKpi.DataSource = collection;
            comboBoxKpi.DisplayMember = "Name";
            comboBoxKpi.ValueMember = "Id";
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void LoadTeam()
        {
            foreach (ITeam item in Teams)
            {
                // Creates a new target if it doesn´t exists
                FindCorrectTarget(item);
            }

            // Filter on correct Key Performance Indicator
            IList<IKpiTarget> view = new List<IKpiTarget>();
            foreach (IKpiTarget target in KpiTargets)
            {
            	if (target.KeyPerformanceIndicator != SelectedKpi) continue;
            	if (comboBoxSite.SelectedIndex > 0)
            	{
            		var s = (ISite) comboBoxSite.SelectedItem;
            		if (s.Equals(target.Team.Site)) view.Add(target);
            	}
            	else
            		view.Add(target);
            }

        	// Sort the result on TeamDescription
            var query = view.Where(kpiTarget => kpiTarget.Team.IsChoosable).ToList().OrderBy(kpiTarget => kpiTarget.TeamDescription);
		    
            _source.Clear();
            foreach (IKpiTarget target in query)
            {
                _source.Add(target);
            }

            ReadOnlyCollection<SFGridColumnBase<IKpiTarget>> x = configureGrid();
            _columnGridHelper = new SFGridColumnGridHelper<IKpiTarget>(gridControl1, x, _source)
                                	{AllowExtendedCopyPaste = true};
            
            gridControl1.Model.ColWidths.ResizeToFit(GridRangeInfo.Cols(0, gridControl1.ColCount), GridResizeToFitOptions.IncludeHeaders);
        }

        private ReadOnlyCollection<SFGridColumnBase<IKpiTarget>> configureGrid()
        {

            addCellModels();
            IList<SFGridColumnBase<IKpiTarget>> gridColumns = new List<SFGridColumnBase<IKpiTarget>>();

            gridControl1.Rows.HeaderCount = 0;
            // Grid must have a Header column
            gridColumns.Add(new SFGridRowHeaderColumn<IKpiTarget>(string.Empty));
            gridColumns.Add(new SFGridReadOnlyTextColumn<IKpiTarget>("Team.SiteAndTeam", UserTexts.Resources.Team));
            gridColumns.Add(new SFGridNumericCellColumn<IKpiTarget>("TargetValue", UserTexts.Resources.Target, null, "NumericCell",30));
            gridColumns.Add(new SFGridColorPickerColumn<IKpiTarget>("LowerThanMinColor", UserTexts.Resources.Color, null));
            gridColumns.Add(new SFGridNumericCellColumn<IKpiTarget>("MinValue", "<", null, "NumericCell", 30));
            gridColumns.Add(new SFGridColorPickerColumn<IKpiTarget>("BetweenColor", UserTexts.Resources.Color, null));
            gridColumns.Add(new SFGridNumericCellColumn<IKpiTarget>("MaxValue", ">", null, "NumericCell", 30));
            gridColumns.Add(new SFGridColorPickerColumn<IKpiTarget>("HigherThanMaxColor", UserTexts.Resources.Color, null));

            gridColumns.AppendAuditColumns();

            gridControl1.RowCount = gridRowCount();
            gridControl1.ColCount = gridColumns.Count - 1;  //col index starts on 0
            return new ReadOnlyCollection<SFGridColumnBase<IKpiTarget>>(gridColumns);
        }

        private void addCellModels()
        {
            if (!gridControl1.CellModels.ContainsKey("ColorPickerCell"))
            {
                gridControl1.CellModels.Add("ColorPickerCell", new ColorPickerCellModel(gridControl1.Model));
            }

            if (!gridControl1.CellModels.ContainsKey("NumericCell"))
            {
                gridControl1.CellModels.Add("NumericCell", new NumericCellModel(gridControl1.Model));
            }
        }

        private int gridRowCount()
        {
            return _source.Count + gridControl1.Rows.HeaderCount;
        }

        private void LoadSites()
        {
            var repSite = new SiteRepository(_unitOfWork);
            IList<ISite> lst = repSite.LoadAll();
            comboBoxSite.Sorted = false;
            comboBoxSite.DisplayMember = "Description";
            comboBoxSite.ValueMember = "Id";
            comboBoxSite.Items.Clear();

            IEnumerable<ISite> query = lst.OrderBy(s => s.Description.Name);

            foreach (ISite item in query)
            {
                comboBoxSite.Items.Add(item);
            }
            var itmAll = new AllSites(UserTexts.Resources.AllSelection);

            comboBoxSite.Items.Insert(0, itmAll);
            comboBoxSite.SelectedItem = itmAll;
        }

        private void FindCorrectTarget(ITeam team)
        {
            if (SelectedKpi == null) return;

            var rep = new KpiTargetRepository(_unitOfWork);
            foreach (KpiTarget item in KpiTargets)
            {
                if (team.Equals(item.Team) && SelectedKpi.Equals(item.KeyPerformanceIndicator))
                {
                    return;
                }
            }
            var newTarget = new KpiTarget
                            	{
                            		KeyPerformanceIndicator = SelectedKpi,
                            		Team = team,
                            		TargetValue = SelectedKpi.DefaultTargetValue,
                            		MinValue = SelectedKpi.DefaultMinValue,
                            		MaxValue = SelectedKpi.DefaultMaxValue,
                            		BetweenColor = SelectedKpi.DefaultBetweenColor,
                            		LowerThanMinColor = SelectedKpi.DefaultLowerThanMinColor,
                            		HigherThanMaxColor = SelectedKpi.DefaultHigherThanMaxColor
                            	};
        	rep.Add(newTarget);
            KpiTargets.Add(newTarget);
            return;
        }

        private class AllSites
        {
            public AllSites(string name)
            {
                Id = Guid.NewGuid();
                Description = name;
            }

            public Guid Id { get; private set; }
            public string Description { get; private set; }
        }

        public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
        {
        }

        public ViewType ViewType
        {
            get { return ViewType.KpiSettings; }
        }
    }
}
