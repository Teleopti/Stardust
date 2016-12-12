﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.Practices.CompositeUI.SmartParts;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Common.UI.SmartPartControls.SmartParts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.SmartParts.Forecasting
{
    [SmartPart]
    public partial class DetailedSmartPart : SmartPartBase, IDrawingBehavior, ILocalized
    {
	    private IList<NamedEntity> _workloadNames;
        private ForecastGraphsControl _forecasterControl;
        private EntityUpdateInformation _lastUpdatedByText;
        private readonly NavigationControl _navigator;
        private ISkill _skill;
        private readonly ISmartPartModel _smartPartModel;
        private readonly IDrawSmartPart _drawSmartPart;
        private List<NamedEntity> _lastUpatedScenarios;
        private IDictionary<IScenario, EntityUpdateInformation> _allLastUpdatedByText;
        private IDictionary<Guid, IDictionary<Guid, IList<IForecastProcessReport>>> _allDetailedForecasting;
        private const string DateFormat = "g";

        public DateOnlyPeriod ForecastPeriod { get; set; }

        public DetailedSmartPart()
        {
	        InitializeComponent();
            if (!DesignMode) SetTexts();

            ForecastPeriod = new DateOnlyPeriod(DateTime.Today.Year, 1, 1, DateTime.Today.Year, 12, 31);

            _navigator = new NavigationControl(this, ForecastPeriod);
            _navigator.Dock = DockStyle.Fill;
            _smartPartModel = new SmartPartModel();
            _drawSmartPart = new DrawSmartPart();
        }

        //private void DetailedSmartPart_Load(object sender, EventArgs e)
        //{
        //    RegisterForMessageBrokerEvents(typeof(IForecastData));
        //}

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
                    IDictionary<Guid, IList<IForecastProcessReport>> detailed = _allDetailedForecasting[_lastUpatedScenarios[index / rowsOfOneScenario].Id];
                    int workloadIndex = (index - 1) % rowsOfOneScenario;
                    _drawSmartPart.DrawForecasts(drawProperties,
                                  detailed[_workloadNames[workloadIndex].Id][0].PeriodCollection,
                                  ForecastPeriod, workloadIndex);
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
                    IDictionary<Guid, IList<IForecastProcessReport>> detailed = _allDetailedForecasting[scenarioId];
                    int workloadIndex = (index - 1) % rowsOfOneScenario;
                    toolTipInfo = _drawSmartPart.GetProgressGraphToolTip(drawPositionAndWidth,
                                                                         detailed[
                                                                             _workloadNames[workloadIndex].Id][
                                                                                 0].PeriodCollection, ForecastPeriod,  cursorX);
                }
                else
                {
                    if (index % rowsOfOneScenario == 0)
                    {
                        var scenarioId = _lastUpatedScenarios[index / rowsOfOneScenario].Id;
	                    IScenario scenario;
						using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
							scenario = _smartPartModel.GetScenarioById(scenarioId);
                        EntityUpdateInformation updateInformation;
                        _allLastUpdatedByText.TryGetValue(scenario, out updateInformation);
                        toolTipInfo = _drawSmartPart.GetScenarioToolTip(drawPositionAndWidth, updateInformation, cursorX);
                    }
                    else
                    {
                        var scenarioId = _lastUpatedScenarios[index / rowsOfOneScenario].Id;
                        IDictionary<Guid, IList<IForecastProcessReport>> detailed = _allDetailedForecasting[scenarioId];
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
				using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					_allLastUpdatedByText = _smartPartModel.SetLastUpdatedWorkloadDetailedValuesOfAllScenarios(false);
					_allDetailedForecasting = _smartPartModel.ProcessAllDetailedForecasting(ForecastPeriod);
	            }
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
                    lastUpdated = _lastUpdatedByText.LastUpdate.Value.ToString(DateFormat, TeleoptiPrincipal.CurrentPrincipal.Regional.Culture);

                labelLastUpdated.Text = string.Format(CultureInfo.CurrentCulture, "{0}: {1}", UserTexts.Resources.LastUpdated, lastUpdated);
                labelChangedBy.Text = string.Format(CultureInfo.CurrentCulture, "{0}: {1}", UserTexts.Resources.ChangedBy,
                                                    (_lastUpdatedByText.Name.HasValue)
                                                        ? _lastUpdatedByText.Name.Value.ToString()
                                                        : string.Empty);
            }
            labelDefaultScenarioName.Text = string.Format(CultureInfo.CurrentCulture, "{0}",UserTexts.Resources.DefaultScenario);
            labelTitle.Text = string.Format(CultureInfo.CurrentCulture, "{0}: {1}", UserTexts.Resources.DetailedForecastSmartPart, _skill.Name);
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
        }
    }
}