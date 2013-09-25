using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.Practices.CompositeUI.SmartParts;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Common.UI.SmartPartControls.SmartParts;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.SmartParts.Forecasting
{
    [SmartPart]
    public partial class TemplateSmartPart : SmartPartBase, IDrawingBehavior, ILocalized
    {
        private ForecastGraphsControl _forecasterControl;
        private IList<EntityUpdateInformation> _workloadDetails;
        private IList<NamedEntity> _workloadNames;
        private ISkill _skill;
        private readonly ISmartPartModel _smartPartModel;
        private readonly IDrawSmartPart _drawSmartPart;
        private const int DefaultNamesCol = 150;
        private const int DefaultColCount = 3;
        private const string DateFormat = "g";
        private const int NamesColumnWidth = 100;

        public TemplateSmartPart()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
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
            AddTitleLabel();
            AddForecastGraphsControl();
        }

        public void DrawProgressGraphs(IDrawProperties drawProperties)
        {
            if (drawProperties == null) throw new ArgumentNullException("drawProperties");
            DrawLastUpdatedInfoForTemplateSmartPart(drawProperties, drawProperties.RowIndex - 1);
        }

        private void DrawLastUpdatedInfoForTemplateSmartPart(IDrawProperties drawProperties, int index)
        {
            if (index == 0)
            {
                drawProperties.Graphics.DrawString(UserTexts.Resources.LastUpdated, _drawSmartPart.DefaultFont, Brushes.Black,
                                DefaultNamesCol, drawProperties.Bounds.Y);

                drawProperties.Graphics.DrawString(UserTexts.Resources.ChangedBy, _drawSmartPart.DefaultFont, Brushes.Black,
                                    (NamesColumnWidth + DefaultNamesCol), drawProperties.Bounds.Y);
            }

            if (index > 0 && (index - 1) < _workloadDetails.Count)
            {
                EntityUpdateInformation workloadDetail = _workloadDetails[index - 1];

                string updatedOn = string.Empty;
                if (workloadDetail.LastUpdate.HasValue && StateHolderReader.IsInitialized && StateHolderReader.Instance.StateReader.IsLoggedIn)
                    updatedOn = workloadDetail.LastUpdate.Value.ToString(DateFormat, TeleoptiPrincipal.Current.Regional.Culture);

                drawProperties.Graphics.DrawString(updatedOn, _drawSmartPart.DefaultFont, Brushes.Black,
                                    DefaultNamesCol, drawProperties.Bounds.Y);

                string updatedBy = string.Empty;
                if (workloadDetail.Name.HasValue) updatedBy = workloadDetail.Name.Value.ToString();

                drawProperties.Graphics.DrawString(updatedBy, _drawSmartPart.DefaultFont, Brushes.Black,
                                    (NamesColumnWidth + DefaultNamesCol), drawProperties.Bounds.Y);
            }
        }

        public void DrawNames(IDrawProperties drawProperties)
        {
            if (drawProperties == null) throw new ArgumentNullException("drawProperties");

            int index = drawProperties.RowIndex - 1;
            if ((_workloadNames != null) && (index > 0 && (index - 1) < _workloadDetails.Count))
            {
                _drawSmartPart.DrawWorkloadNames(drawProperties, _workloadNames, (index - 1));
            }
        }

        public ToolTipInfo SetTooltip(IDrawPositionAndWidth drawPositionAndWidth, int cursorX)
        {
            if (drawPositionAndWidth == null) throw new ArgumentNullException("drawPositionAndWidth");
            ToolTipInfo toolTipInfo = new ToolTipInfo();
            int index = drawPositionAndWidth.RowIndex - 1;

            if (_workloadNames != null && (index > 0 && (index - 1) < _workloadDetails.Count))
            {
                toolTipInfo = _drawSmartPart.GetWorkloadToolTip(drawPositionAndWidth,
                                                                                 _workloadNames[index-1].Name, cursorX);
                
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
                _smartPartModel.ProcessTemplates();
                _workloadDetails = _smartPartModel.WorkloadUpdatedInfo;
                _workloadNames = _smartPartModel.WorkloadNames;
            }
        }

        private void AddTitleLabel()
        {
            labelTitle.Text = string.Format(CultureInfo.CurrentCulture, "{0}: {1}",UserTexts.Resources.TemplatesSmartpart, _skill.Name);
        }

        private void AddForecastGraphsControl()
        {
            if (_forecasterControl == null)
            {
                _forecasterControl = new ForecastGraphsControl(this, _drawSmartPart);
                _forecasterControl.Dock = DockStyle.Fill;
                panel1.Controls.Add(_forecasterControl);

                _forecasterControl.Timeline = false;
                _forecasterControl.ProgressColumn.ColCount = DefaultColCount;
                _forecasterControl.ProgressColumn.ColCount = 3;
                _forecasterControl.ProgressColumn.ColWidths[1] = NamesColumnWidth;
                _forecasterControl.ProgressColumn.ColWidths[2] = DefaultNamesCol;
                _forecasterControl.ProgressColumn.ColWidths[3] = _forecasterControl.ProgressColumn.Width - (DefaultNamesCol + 75);
            }

            _forecasterControl.ProgressColumn.RowCount = _workloadDetails.Count + 1;
            _forecasterControl.ProgressColumn.Invalidate();
        }
    }
}