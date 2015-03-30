using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.QuickForecast
{
	public partial class SelectHistoricalDateRange : BaseUserControl, IPropertyPageNoRoot<QuickForecastCommandDto>
    {
		private QuickForecastCommandDto _stateObj;
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
        }

		public void Populate(QuickForecastCommandDto stateObj)
		{
			HistoricalFromTo.EnableNullDates = false;
            _stateObj = stateObj;
        }

        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);

           // var exportModel = _stateObj.ExportMultisiteSkillToSkillCommandModel;
			HistoricalFromTo.WorkPeriodStart = new DateOnly(_stateObj.StatisticPeriod.StartDate.DateTime);
			HistoricalFromTo.WorkPeriodEnd = new DateOnly(_stateObj.StatisticPeriod.EndDate.DateTime);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool Depopulate(QuickForecastCommandDto stateObj)
        {
	        stateObj.StatisticPeriod = new DateOnlyPeriodDto
		        {
			        StartDate = new DateOnlyDto {DateTime = HistoricalFromTo.WorkPeriodStart.Date},
			        EndDate = new DateOnlyDto {DateTime = HistoricalFromTo.WorkPeriodEnd.Date}
		        };
            return true;	
        }

        public void SetEditMode()
        {
        }

        public string PageName
        {
			get { return Resources.SelectHistoricalData; }
        }

        public ICollection<string> ErrorMessages
        {
            get { return _errorMessages; }
        }

		public override string HelpId
		{
			get
			{
				return "Help";
			}
		}
    }
}
