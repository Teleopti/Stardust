using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.Win.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Optimization
{
    public partial class AdvancedPreferencesPanel : BaseUserControl, IDataExchange
    {
        public IAdvancedPreferences Preferences { get; private set; }

        public AdvancedPreferencesPanel()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        public void Initialize(IAdvancedPreferences preferences)
        {
            Preferences = preferences;
            ExchangeData(ExchangeDataOption.DataSourceToControls);
            setInitialControlStatus();
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
                setDataInControls();
            }
            else
            {
                getDataFromControls();
            }
        }

        #endregion

        private void getDataFromControls()
        {
            if (radioButtonStandardDeviation.Checked)
                Preferences.TargetValueCalculation = TargetValueOptions.StandardDeviation;
            if (radioButtonRootMeanSquare.Checked)
                Preferences.TargetValueCalculation = TargetValueOptions.RootMeanSquare;
            if (radioButtonTeleopti.Checked)
                Preferences.TargetValueCalculation = TargetValueOptions.Teleopti;


            Preferences.UseIntraIntervalDeviation = checkBoxUseIntraIntervalDeviation.Checked;
            Preferences.UseTweakedValues = checkBoxUseTweakedValues.Checked;

            Preferences.UseMinimumStaffing = checkBoxMinimumStaffing.Checked;
            Preferences.UseMaximumStaffing = checkBoxMaximumStaffing.Checked;
				if (checkBoxMaximumSeats.Checked && checkBoxDoNotBreakMaximumSeats.Checked)
					Preferences.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak;
				else if (checkBoxMaximumSeats.Checked)
					Preferences.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeats;
				else if (!checkBoxMaximumSeats.Checked)
					Preferences.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.DoNotConsiderMaxSeats;
        	   Preferences.UseAverageShiftLengths = checkBoxUseAverageShiftLengths.Checked;
            Preferences.RefreshScreenInterval = (int)numericUpDownRefreshRate.Value;
        }

        private void setDataInControls()
        {
            switch (Preferences.TargetValueCalculation)
            {
                case TargetValueOptions.StandardDeviation:
                    radioButtonStandardDeviation.Checked = true;
                    break;
                case TargetValueOptions.RootMeanSquare:
                    radioButtonRootMeanSquare.Checked = true;
                    break;
                case TargetValueOptions.Teleopti:
                    radioButtonTeleopti.Checked = true;
                    break;
            }

            checkBoxUseIntraIntervalDeviation.Checked = Preferences.UseIntraIntervalDeviation;
            checkBoxUseTweakedValues.Checked = Preferences.UseTweakedValues;

            checkBoxMinimumStaffing.Checked = Preferences.UseMinimumStaffing;
            checkBoxMaximumStaffing.Checked = Preferences.UseMaximumStaffing;
				if (Preferences.UserOptionMaxSeatsFeature == MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak)
				{
					checkBoxMaximumSeats.Checked = true;
					checkBoxDoNotBreakMaximumSeats.Enabled = true;
					checkBoxDoNotBreakMaximumSeats.Checked = true;
				}
				else if (Preferences.UserOptionMaxSeatsFeature == MaxSeatsFeatureOptions.ConsiderMaxSeats)
				{
					checkBoxMaximumSeats.Checked = true;
					checkBoxDoNotBreakMaximumSeats.Enabled = true;
				}
				else if (Preferences.UserOptionMaxSeatsFeature == MaxSeatsFeatureOptions.DoNotConsiderMaxSeats)
				{
					checkBoxMaximumSeats.Checked = false;
					checkBoxDoNotBreakMaximumSeats.Enabled = false;
					checkBoxDoNotBreakMaximumSeats.Checked = false;
				}
	        checkBoxUseAverageShiftLengths.Checked = Preferences.UseAverageShiftLengths;

	        numericUpDownRefreshRate.Value = Preferences.RefreshScreenInterval;
        }

        private void checkBoxMaximumSeats_CheckedChanged(object sender, System.EventArgs e)
        {
            setCheckBoxDoNotBreakMaximumSeatsStatus();
        }

        private void setCheckBoxDoNotBreakMaximumSeatsStatus()
        {
            checkBoxDoNotBreakMaximumSeats.Enabled = checkBoxMaximumSeats.Checked;
        }

        private void setInitialControlStatus()
        {
            setCheckBoxDoNotBreakMaximumSeatsStatus();
        }
    }
}
