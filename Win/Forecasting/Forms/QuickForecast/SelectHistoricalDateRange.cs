using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.WinCode.Forecasting.QuickForecastPages;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.QuickForecast
{
	public partial class SelectHistoricalDateRange : BaseUserControl, IPropertyPageNoRoot<QuickForecastModel>
    {
		private QuickForecastModel _stateObj;
        private readonly ICollection<string> _errorMessages = new List<string>();

        public SelectHistoricalDateRange()
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

		public void Populate(QuickForecastModel stateObj)
        {
            _stateObj = stateObj;
        }

        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);

           // var exportModel = _stateObj.ExportMultisiteSkillToSkillCommandModel;
			reportDateFromToSelector1.WorkPeriodStart = new DateOnly(_stateObj.StatisticPeriod.StartDate.DateTime);
			reportDateFromToSelector1.WorkPeriodEnd = new DateOnly(_stateObj.StatisticPeriod.EndDate.DateTime);
        }

		public bool Depopulate(QuickForecastModel stateObj)
        {
	        stateObj.StatisticPeriod = new DateOnlyPeriodDto
		        {
			        StartDate = new DateOnlyDto {DateTime = reportDateFromToSelector1.WorkPeriodStart},
			        EndDate = new DateOnlyDto {DateTime = reportDateFromToSelector1.WorkPeriodEnd}
		        };
            return true;	
        }

        public void SetEditMode()
        {
        }

        public string PageName
        {
            get { return  "xxHistorical data range"; }
        }

        public ICollection<string> ErrorMessages
        {
            get { return _errorMessages; }
        }
    }
}
