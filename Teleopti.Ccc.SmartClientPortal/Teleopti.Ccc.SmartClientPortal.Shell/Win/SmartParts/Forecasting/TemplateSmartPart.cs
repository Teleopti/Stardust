using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.Practices.CompositeUI.SmartParts;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.SmartParts.Forecasting
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
        }

        public void DrawNames(IDrawProperties drawProperties)
        {
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
                _forecasterControl.Dock = DockStyle.Top;
                panel1.Controls.Add(_forecasterControl);

                _forecasterControl.Timeline = false;
                _forecasterControl.ProgressColumn.ColCount = DefaultColCount;
                _forecasterControl.ProgressColumn.ColCount = 3;
                _forecasterControl.ProgressColumn.ColWidths[1] = NamesColumnWidth;
                _forecasterControl.ProgressColumn.ColWidths[2] = DefaultNamesCol;
                _forecasterControl.ProgressColumn.ColWidths[3] = _forecasterControl.ProgressColumn.Width - (DefaultNamesCol + 75);
	            _forecasterControl.ProgressColumn.ReadOnly = false;
            }

            _forecasterControl.ProgressColumn.RowCount = _workloadDetails.Count + 1;
	        DrawInfo();
        }

	    private void DrawInfo()
	    {
			 _forecasterControl.ProgressColumn.Model[1, 2].CellValue = UserTexts.Resources.LastUpdated;
			 _forecasterControl.ProgressColumn.Model[1, 3].CellValue = UserTexts.Resources.ChangedBy; 
		    for (int i = 2; i <= _workloadDetails.Count + 1; i++)
		    {
				 EntityUpdateInformation workloadDetail = _workloadDetails[i - 2];

				 string updatedOn = string.Empty;
				 if (workloadDetail.LastUpdate.HasValue)
					 updatedOn = workloadDetail.LastUpdate.Value.ToString(DateFormat, TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.Culture);
				 
				 string updatedBy = string.Empty;
				 if (workloadDetail.Name.HasValue) updatedBy = workloadDetail.Name.Value.ToString();

				 
				 _forecasterControl.ProgressColumn.Model[i, 1].CellValue = _workloadNames[i-2].Name;
				 _forecasterControl.ProgressColumn.Model[i, 2].CellValue = updatedOn;
				 _forecasterControl.ProgressColumn.Model[i, 3].CellValue = updatedBy;
				
		    }
			 _forecasterControl.ProgressColumn.ColWidths.ResizeToFit(GridRangeInfo.Table());
		 }

		 private void TemplateSmartPart_Enter(object sender, EventArgs e)
		 {
			 DrawInfo();
		 }

    }
}