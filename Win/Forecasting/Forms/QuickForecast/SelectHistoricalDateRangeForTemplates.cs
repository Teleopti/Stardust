using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.QuickForecast
{
	public partial class SelectHistoricalDateRangeForTemplates : BaseUserControl, IPropertyPageNoRoot<QuickForecastCommandDto>
    {
		private QuickForecastCommandDto _stateObj;
        private readonly ICollection<string> _errorMessages = new List<string>();

		public SelectHistoricalDateRangeForTemplates()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
                setColors();
        }

        private void setColors()
        {
            BackColor = ColorHelper.WizardBackgroundColor();
            label1.BackColor = ColorHelper.WizardPanelBackgroundColor();
        }

		public void Populate(QuickForecastCommandDto stateObj)
        {
            _stateObj = stateObj;
        }

        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);

           // var exportModel = _stateObj.ExportMultisiteSkillToSkillCommandModel;
			TemplatesDatesFromTo.WorkPeriodStart = new DateOnly(_stateObj.TemplatePeriod.StartDate.DateTime);
			TemplatesDatesFromTo.WorkPeriodEnd = new DateOnly(_stateObj.TemplatePeriod.EndDate.DateTime);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool Depopulate(QuickForecastCommandDto stateObj)
        {
	        stateObj.TemplatePeriod = new DateOnlyPeriodDto
		        {
			        StartDate = new DateOnlyDto {DateTime = TemplatesDatesFromTo.WorkPeriodStart},
			        EndDate = new DateOnlyDto {DateTime = TemplatesDatesFromTo.WorkPeriodEnd}
		        };
            return true;	
        }

        public void SetEditMode()
        {
        }

        public string PageName
        {
            get { return  "xxTemplates historical data range"; }
        }

        public ICollection<string> ErrorMessages
        {
            get { return _errorMessages; }
        }
    }
}
