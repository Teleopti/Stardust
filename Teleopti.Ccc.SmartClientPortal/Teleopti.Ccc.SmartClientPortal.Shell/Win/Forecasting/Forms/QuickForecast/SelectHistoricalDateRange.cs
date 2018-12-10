using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.QuickForecastPages;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.QuickForecast
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
		}

		public void Populate(QuickForecastModel stateObj)
		{
			HistoricalFromTo.EnableNullDates = false;
			_stateObj = stateObj;
		}

		protected override void OnLoad(System.EventArgs e)
		{
			base.OnLoad(e);

			// var exportModel = _stateObj.ExportMultisiteSkillToSkillCommandModel;
			HistoricalFromTo.WorkPeriodStart = _stateObj.StatisticPeriod.StartDate;
			HistoricalFromTo.WorkPeriodEnd = _stateObj.StatisticPeriod.EndDate;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool Depopulate(QuickForecastModel stateObj)
		{
			stateObj.StatisticPeriod = new DateOnlyPeriod
				(
					HistoricalFromTo.WorkPeriodStart,
					HistoricalFromTo.WorkPeriodEnd
				);
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
