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
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.SmartParts.Forecasting
{
    [SmartPart]
    public partial class ValidationSmartPart : SmartPartBase, IDrawingBehavior, ILocalized
    {
        private IDictionary<Guid, IList<IForecastProcessReport>> _workDays;
        private IList<NamedEntity> _workloadNames;
        private ForecastGraphsControl _forecasterControl;
        private EntityUpdateInformation _lastUpdatedByText;
        private readonly NavigationControl _navigator;
        private ISkill _skill;
        private readonly ISmartPartModel _smartPartModel;
        private readonly IDrawSmartPart _drawSmartPart;
        private const string DateFormat = "g";

        public DateOnlyPeriod ForecastPeriod { get; set; }

        public ValidationSmartPart()
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

        public void DrawProgressGraphs(IDrawProperties drawProperties)
        {
            if (drawProperties == null) throw new ArgumentNullException("drawProperties");
            int index = drawProperties.RowIndex - 1;
            if (index < _workDays.Count && _workloadNames != null)
            {
                _drawSmartPart.DrawForecasts(drawProperties,
                                                        _workDays[_workloadNames[index].Id][0].PeriodCollection,
                                                       ForecastPeriod, index);
            }
        }

        public void DrawNames(IDrawProperties drawProperties)
        {
            if (drawProperties == null) throw new ArgumentNullException("drawProperties");
            if (_workloadNames != null)
            {
                int index = drawProperties.RowIndex - 1;
                _drawSmartPart.DrawWorkloadNames(drawProperties, _workloadNames, index);
            }
        }

        public ToolTipInfo SetTooltip(IDrawPositionAndWidth drawPositionAndWidth, int cursorX)
        {
            if (drawPositionAndWidth == null) throw new ArgumentNullException("drawPositionAndWidth");
            ToolTipInfo toolTipInfo = new ToolTipInfo();
            int index = drawPositionAndWidth.RowIndex - 1;
            if (index < _workDays.Count && _workloadNames != null)
            {
                if (cursorX >= drawPositionAndWidth.ProgressStartPosition)
                {
                    toolTipInfo = _drawSmartPart.GetProgressGraphToolTip(drawPositionAndWidth,
                                                                       _workDays[_workloadNames[index].Id][0].PeriodCollection, ForecastPeriod,
                                                                        cursorX);
                }
                else
                {
                    toolTipInfo = _drawSmartPart.GetWorkloadToolTip(drawPositionAndWidth,
                                                                                 _workloadNames[index].Name, cursorX);
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
            _skill = _smartPartModel.Skill;
            if (_skill != null)
            {
                _lastUpdatedByText = _smartPartModel.SetLastUpdatedValidatedVolumnDayValues();
                _workDays = _smartPartModel.ProcessValidations(ForecastPeriod);
                _workloadNames = _smartPartModel.WorkloadNames;
            }
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
            _forecasterControl.ProgressColumn.RowCount = _workDays.Count;
            _forecasterControl.ProgressColumn.Invalidate();
        }

        private void AddTitleAndLastUpdatedInfoLabel()
        {
            if (_lastUpdatedByText != null)
            {
                string lastUpdated = string.Empty;
                if (_lastUpdatedByText.LastUpdate.HasValue)
                    lastUpdated = _lastUpdatedByText.LastUpdate.Value.ToString(DateFormat,
                                                                              TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture);

                labelLastUpdated.Text = string.Format(CultureInfo.CurrentCulture, "{0}: {1}", Resources.LastUpdated, lastUpdated);
                labelChangedBy.Text = string.Format(CultureInfo.CurrentCulture, "{0}: {1}", Resources.ChangedBy,
                                                    (_lastUpdatedByText.Name.HasValue)
                                                        ? _lastUpdatedByText.Name.Value.ToString()
                                                        : string.Empty);
            }
            labelTitle.Text = string.Format(CultureInfo.CurrentCulture, "{0}: {1}", Resources.Validation, _skill.Name);
        }
    }
}