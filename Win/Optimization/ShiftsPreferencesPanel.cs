using Teleopti.Ccc.Win.Common;

using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Optimization
{
	
    public partial class ShiftsPreferencesPanel : BaseUserControl, IDataExchange
    {

        public IShiftPreferences Preferences { get; private set; }

        public ShiftsPreferencesPanel()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public void Initialize(
            IShiftPreferences extraPreferences)
        {
            Preferences = extraPreferences;
			ExchangeData(ExchangeDataOption.DataSourceToControls);
            
        }

        #region IDataExchange Members

        public bool ValidateData(ExchangeDataOption direction)
        {
            return true;
        }

        public void ExchangeData(ExchangeDataOption direction)
        {
            if (direction == ExchangeDataOption.DataSourceToControls)
            {
               
                setDataToControls();
            }
            else
            {
                getDataFromControls();
            }
        }

        #endregion

        
        private void getDataFromControls()
        {
           

            Preferences.KeepShiftCategories = checkBoxKeepShiftCategories.Checked;
            Preferences.KeepStartTimes = checkBoxKeepEndTimes.Checked;
            Preferences.KeepEndTimes = checkBoxKeepStartTimes.Checked;
            Preferences.KeepShifts = checkBoxKeepShifts.Checked;
 
            Preferences.KeepShiftsValue = (double)numericUpDownKeepShifts.Value  / 100;
        }

        private void setDataToControls()
        {
            


            checkBoxKeepShiftCategories.Checked = Preferences.KeepShiftCategories;
            checkBoxKeepEndTimes.Checked = Preferences.KeepEndTimes;
            checkBoxKeepStartTimes.Checked = Preferences.KeepStartTimes;
            checkBoxKeepShifts.Checked = Preferences.KeepShifts;

            numericUpDownKeepShifts.Value = (decimal) Preferences.KeepShiftsValue * 100;
        }

       

        //private void setRadioButtonsStatus()
        //{
        //    radioButtonBetweenDayOff.Enabled = checkBoxBlock.Checked;
        //    radioButtonSchedulePeriod.Enabled = checkBoxBlock.Checked;
        //}

    }



}
