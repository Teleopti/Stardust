using System.Collections.Generic;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.WinCode.Forecasting.QuickForecastPages;

namespace Teleopti.Ccc.Win.Forecasting.Forms.QuickForecast
{
	public partial class SelectTemplateSmoothing : BaseUserControl, IPropertyPageNoRoot<QuickForecastModel>
    {
		private QuickForecastModel _stateObj;
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

		public void Populate(QuickForecastModel stateObj)
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

		public bool Depopulate(QuickForecastModel stateObj)
		{
			stateObj.UpdateStandardTemplates = checkBoxAdv1.Checked;
			stateObj.SmoothingStyle = (int)comboBoxSmoothing.SelectedValue;
            return true;	
        }

        public void SetEditMode()
        {
        }

        public string PageName
        {
            get { return  "xxSmoothing style"; }
        }

        public ICollection<string> ErrorMessages
        {
            get { return _errorMessages; }
        }
			
		private IList<SmoothingValue> smoothingValues()
		{
			return new List<SmoothingValue>
			{
			    new SmoothingValue{Smoothing = 0, Text = Resources.None},
			    new SmoothingValue{Smoothing = 3, Text = "xxLittle"},
			    new SmoothingValue{Smoothing = 5, Text = "xxMedium"},
			    new SmoothingValue{Smoothing = 7, Text = "xxMuch"}
			} ;
		}

		internal class SmoothingValue
		{
			public int Smoothing { get; set; }
			public string Text { get; set; }
		}

		private void checkBoxAdv1CheckStateChanged(object sender, System.EventArgs e)
		{
			comboBoxSmoothing.Enabled = checkBoxAdv1.Checked;
		}
	}

	
}
