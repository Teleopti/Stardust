using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;

namespace Teleopti.Ccc.Win.Forecasting.Forms.QuickForecast
{
	public partial class SelectTemplateSmoothing : BaseUserControl, IPropertyPageNoRoot<QuickForecastCommandDto>
    {
		private QuickForecastCommandDto _stateObj;
        private readonly ICollection<string> _errorMessages = new List<string>();

		public SelectTemplateSmoothing()
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
			//load
			// (None, 3, 5, 7, default to 5) (Perhaps we should call them None, Little, Medium, Much)
	        comboBoxSmoothing.ValueMember = "Smoothing";
	        comboBoxSmoothing.DisplayMember = "Text";
	        comboBoxSmoothing.DataSource = smoothingValues();
	        comboBoxSmoothing.SelectedValue = _stateObj.SmoothingStyle;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool Depopulate(QuickForecastCommandDto stateObj)
		{
			stateObj.SmoothingStyle = (int)comboBoxSmoothing.SelectedValue;
            return true;	
        }

        public void SetEditMode()
        {
        }

        public string PageName
        {
            get { return  Resources.Smoothing; }
        }

        public ICollection<string> ErrorMessages
        {
            get { return _errorMessages; }
        }
			
		private IList<SmoothingValue> smoothingValues()
		{
			return new List<SmoothingValue>
			{
			    new SmoothingValue{Smoothing = 1, Text = Resources.None},
			    new SmoothingValue{Smoothing = 3, Text = Resources.Little},
			    new SmoothingValue{Smoothing = 5, Text = Resources.Medium},
			    new SmoothingValue{Smoothing = 7, Text = Resources.Much}
			} ;
		}

		internal class SmoothingValue
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
			public int Smoothing { get; set; }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
			public string Text { get; set; }
		}

	}
}
