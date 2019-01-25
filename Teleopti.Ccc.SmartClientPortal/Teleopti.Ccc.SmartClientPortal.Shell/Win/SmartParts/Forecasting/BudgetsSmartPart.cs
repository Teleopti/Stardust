using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.Practices.CompositeUI.SmartParts;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.SmartParts.Forecasting
{
    [SmartPart]
    public partial class BudgetsSmartPart : SmartPartBase, IDrawingBehavior, ILocalized
    {
        private IDictionary<Guid, IDictionary<Guid, IList<IForecastProcessReport>>> _allBudgetForecasting;
        private IList<NamedEntity> _workloadNames;
        private IList<NamedEntity> _lastUpatedScenarios;
        private ForecastGraphsControl _forecasterControl;
        private EntityUpdateInformation _lastUpdatedByText;
        private IDictionary<IScenario, EntityUpdateInformation> _allLastUpdatedByText;
        private readonly NavigationControl _navigator;
        private ISkill _skill;
        private readonly ISmartPartModel _smartPartModel;
        private readonly IDrawSmartPart _drawSmartPart;

        private const string DateFormat = "g";

        public DateOnlyPeriod ForecastPeriod { get; set; }

        public BudgetsSmartPart()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();

            ForecastPeriod = new DateOnlyPeriod(DateTime.Today.Year, 1, 1, DateTime.Today.Year, 12, 31);

            _navigator = new NavigationControl(this, ForecastPeriod);
            _navigator.Dock = DockStyle.Fill;

            _smartPartModel = new SmartPartModel();
            _drawSmartPart = new DrawSmartPart();
        }
        
        public override void OnBackgroundProcess(DoWorkEventArgs e)
        {
            base.OnBackgroundProcess(e);

            InitializeSmartPartModel();
            CollectUpdatedData();
        }

        private void InitializeSmartPartModel()
        {
            _smartPartModel.SkillId = (Guid)SmartPartParameters[0].Value;
        }

        public override void AfterBackgroundProcessCompleted()
        {
            base.AfterBackgroundProcessCompleted();

            AddTitleAndLastUpdatedInfoLabel();
            AddForecastGraphsControl();
        }

        public void DrawNames(IDrawProperties drawProperties)
        {
            if (drawProperties == null) throw new ArgumentNullException("drawProperties");
            if (_lastUpatedScenarios != null && _workloadNames != null)
            {
                _drawSmartPart.DrawScenarioNames(drawProperties, _lastUpatedScenarios, _workloadNames);
            }
        }

        public void DrawProgressGraphs(IDrawProperties drawProperties)
        {
            if (drawProperties == null) throw new ArgumentNullException("drawProperties");
            int totalScenarios = _lastUpatedScenarios.Count;
            int totalWorkloads = _workloadNames.Count;
            int rowsOfOneScenario = totalWorkloads + 1;
            int index = drawProperties.RowIndex - 1;
            if (index >= totalScenarios * rowsOfOneScenario) return;
            if (_lastUpatedScenarios != null && _workloadNames != null)
            {
                if (index % rowsOfOneScenario != 0)
                {
                    IDictionary<Guid, IList<IForecastProcessReport>> budgets = _allBudgetForecasting[_lastUpatedScenarios[index / rowsOfOneScenario].Id];
                    int workloadIndex = (index - 1) % rowsOfOneScenario;
                    _drawSmartPart.DrawForecasts(drawProperties,
                                  budgets[_workloadNames[workloadIndex].Id][0].PeriodCollection, ForecastPeriod, workloadIndex);
                }
            }
        }

        public ToolTipInfo SetTooltip(IDrawPositionAndWidth drawPositionAndWidth, int cursorX)
        {
            if (drawPositionAndWidth == null) throw new ArgumentNullException("drawPositionAndWidth");
            ToolTipInfo toolTipInfo = new ToolTipInfo();
            int totalScenarios = _lastUpatedScenarios.Count;
            int totalWorkloads = _workloadNames.Count;
            int rowsOfOneScenario = totalWorkloads + 1;
            int index = drawPositionAndWidth.RowIndex - 1;
            if (index < totalScenarios * rowsOfOneScenario && _workloadNames != null && _lastUpatedScenarios != null)
            {
                if (cursorX >= drawPositionAndWidth.ProgressStartPosition)
                {
                    if (index % rowsOfOneScenario == 0) return toolTipInfo;
                    var scenarioId = _lastUpatedScenarios[index / rowsOfOneScenario].Id;
                    IDictionary<Guid, IList<IForecastProcessReport>> budgets = _allBudgetForecasting[scenarioId];
                    int workloadIndex = (index - 1) % rowsOfOneScenario;
                    toolTipInfo = _drawSmartPart.GetProgressGraphToolTip(drawPositionAndWidth,
                                                                         budgets[
                                                                             _workloadNames[workloadIndex].Id][
                                                                                 0].PeriodCollection, ForecastPeriod, cursorX);
                }
                else
                {

                    if (index % rowsOfOneScenario == 0)
                    {
                        var scenarioId = _lastUpatedScenarios[index / rowsOfOneScenario].Id;
                        var scenario = _smartPartModel.GetScenarioById(scenarioId);
                        EntityUpdateInformation updateInformation;
                        _allLastUpdatedByText.TryGetValue(scenario, out updateInformation);
                        toolTipInfo = _drawSmartPart.GetScenarioToolTip(drawPositionAndWidth, updateInformation, cursorX);
                    }
                    else
                    {
                        int workloadIndex = (index - 1) % rowsOfOneScenario;
                        toolTipInfo = _drawSmartPart.GetWorkloadToolTip(drawPositionAndWidth,
                                                                                 _workloadNames[workloadIndex].Name, cursorX);
                    }

                    
                }
            }
            return toolTipInfo;
        }

        public void SetTexts()
        {
            new LanguageResourceHelper().SetTexts(this);
        }

        private void CollectUpdatedData()
        {
            _lastUpatedScenarios = new List<NamedEntity>();
            _skill = _smartPartModel.Skill;
            if (_skill != null)
            {
                _allLastUpdatedByText =
                    _smartPartModel.SetLastUpdatedWorkloadDetailedValuesOfAllScenarios(true);
                _allBudgetForecasting = _smartPartModel.ProcessAllBudgetForecasting(ForecastPeriod);
                _allLastUpdatedByText.TryGetValue(_smartPartModel.DefaultScenario, out _lastUpdatedByText);
                _workloadNames = _smartPartModel.WorkloadNames;
                var scenarioList = _smartPartModel.FindLastUpdatedScenarios(_allLastUpdatedByText);
                foreach (IScenario scenario in scenarioList)
                {
                    var scenarioEntity = new NamedEntity
                    {
                        Name = scenario.Description.Name,
                        Id = scenario.Id.GetValueOrDefault(Guid.Empty)
                    };
                    _lastUpatedScenarios.Add(scenarioEntity);
                }
            }
        }

        private void AddTitleAndLastUpdatedInfoLabel()
        {
            if (_lastUpdatedByText != null)
            {
                string lastUpdated = string.Empty;
                if (_lastUpdatedByText.LastUpdate.HasValue)
                    lastUpdated = _lastUpdatedByText.LastUpdate.Value.ToString(DateFormat, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture);
                labelLastUpdated.Text = string.Format(CultureInfo.CurrentCulture, "{0}: {1}", UserTexts.Resources.LastUpdated, lastUpdated);
                labelChangedBy.Text = string.Format(CultureInfo.CurrentCulture, "{0}: {1}",
													UserTexts.Resources.ChangedBy,
                                                    (_lastUpdatedByText.Name.HasValue)
                                                        ? _lastUpdatedByText.Name.Value.ToString()
                                                        : string.Empty);
                labelDefaultScenarioName.Text = string.Format(CultureInfo.CurrentCulture, "{0}", UserTexts.Resources.DefaultScenario);
            }
            labelTitle.Text = string.Format(CultureInfo.CurrentCulture, "{0}: {1}", UserTexts.Resources.LongtermForecast, _skill.Name);
        }

        private void AddForecastGraphsControl()
        {
            if (_forecasterControl == null)
            {
                _forecasterControl = new ForecastGraphsControl(this, _drawSmartPart);
                _forecasterControl.SuspendLayout();
                _forecasterControl.AddNavigator(_navigator);
                _forecasterControl.ResumeLayout();

                _forecasterControl.Dock = DockStyle.Fill;
                panel1.Controls.Add(_forecasterControl);
                _forecasterControl.Timeline = true;
            }

            _forecasterControl.ProgressColumn.RowCount = _lastUpatedScenarios.Count * (_workloadNames.Count + 1);
            _forecasterControl.ProgressColumn.Invalidate();
            _forecasterControl.Refresh();
        }
    }
}