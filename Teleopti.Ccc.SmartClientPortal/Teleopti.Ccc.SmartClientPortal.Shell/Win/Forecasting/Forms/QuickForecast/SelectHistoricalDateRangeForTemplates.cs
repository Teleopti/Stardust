using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.QuickForecastPages;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.QuickForecast
{
	public partial class SelectHistoricalDateRangeForTemplates : BaseUserControl, IPropertyPageNoRoot<QuickForecastModel>
	{
		private QuickForecastModel _stateObj;
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
		}

		public void Populate(QuickForecastModel stateObj)
		{
			TemplatesDatesFromTo.EnableNullDates = false;
			_stateObj = stateObj;
		}

		protected override void OnLoad(System.EventArgs e)
		{
			base.OnLoad(e);

			TemplatesDatesFromTo.WorkPeriodStart = _stateObj.TemplatePeriod.StartDate;
			TemplatesDatesFromTo.WorkPeriodEnd = _stateObj.TemplatePeriod.EndDate;

			//load
			// (None, 3, 5, 7, default to 5) (Perhaps we should call them None, Little, Medium, Much)
			comboBoxSmoothing.ValueMember = "Smoothing";
			comboBoxSmoothing.DisplayMember = "Text";
			comboBoxSmoothing.DataSource = smoothingValues();
			comboBoxSmoothing.SelectedValue = _stateObj.SmoothingStyle;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods",
			MessageId = "0")]
		public bool Depopulate(QuickForecastModel stateObj)
		{
			stateObj.TemplatePeriod = new DateOnlyPeriod
				(
				TemplatesDatesFromTo.WorkPeriodStart,
				TemplatesDatesFromTo.WorkPeriodEnd
				);
			stateObj.SmoothingStyle = (int) comboBoxSmoothing.SelectedValue;
			return true;
		}

		public void SetEditMode()
		{
		}

		public string PageName
		{
			get { return Resources.TemplatesHistoricalDataRange; }
		}

		public ICollection<string> ErrorMessages
		{
			get { return _errorMessages; }
		}

		private static IList<SmoothingValue> smoothingValues()
		{
			return new List<SmoothingValue>
			{
				 new SmoothingValue{Smoothing = 1, Text = Resources.None},
				 new SmoothingValue{Smoothing = 3, Text = Resources.Little},
				 new SmoothingValue{Smoothing = 5, Text = Resources.Medium},
				 new SmoothingValue{Smoothing = 7, Text = Resources.Much}
			};
		}

		internal class SmoothingValue
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
			public int Smoothing { get; set; }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
			public string Text { get; set; }
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
