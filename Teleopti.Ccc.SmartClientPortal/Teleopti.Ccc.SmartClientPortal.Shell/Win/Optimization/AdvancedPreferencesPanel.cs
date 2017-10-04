using System;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Optimization
{
    public partial class AdvancedPreferencesPanel : BaseUserControl, IDataExchange
    {
        public IAdvancedPreferences Preferences { get; private set; }
		private bool _showBreakPreferenceStartTimeByMax46002;

        public AdvancedPreferencesPanel()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

		[RemoveMeWithToggle("showBreakPreferenceStartTimeByMax46002",Toggles.ResourcePlanner_BreakPreferenceStartTimeByMax_46002)]
        public void Initialize(IAdvancedPreferences preferences, bool showBreakPreferenceStartTimeByMax46002)
		{
			_showBreakPreferenceStartTimeByMax46002 = showBreakPreferenceStartTimeByMax46002;
            Preferences = preferences;
            ExchangeData(ExchangeDataOption.DataSourceToControls);
            setInitialControlStatus();
			checkBoxAdvBreakPreferenceStartTimeByMax.Visible = showBreakPreferenceStartTimeByMax46002;
			numericUpDownBreakByHours.Visible = showBreakPreferenceStartTimeByMax46002;
			labelBreakByMaxHours.Visible = showBreakPreferenceStartTimeByMax46002;	
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
			
			if(checkBoxAdvBreakPreferenceStartTimeByMax.Checked && _showBreakPreferenceStartTimeByMax46002)
				Preferences.BreakPreferenceStartTimeByMax = TimeSpan.FromHours((int)numericUpDownBreakByHours.Value);
			else
				Preferences.BreakPreferenceStartTimeByMax = TimeSpan.Zero;
			
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

			checkBoxAdvBreakPreferenceStartTimeByMax.Checked = Preferences.BreakPreferenceStartTimeByMax != TimeSpan.Zero;
			numericUpDownBreakByHours.Enabled = checkBoxAdvBreakPreferenceStartTimeByMax.Checked;
			numericUpDownBreakByHours.Value = Preferences.BreakPreferenceStartTimeByMax == TimeSpan.Zero ? 1 : Preferences.BreakPreferenceStartTimeByMax.Hours;
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

		private void checkBoxAdvBreakPreferenceStartTimeByMax_CheckStateChanged(object sender, System.EventArgs e)
		{
			numericUpDownBreakByHours.Enabled = checkBoxAdvBreakPreferenceStartTimeByMax.Checked;
			if (!checkBoxAdvBreakPreferenceStartTimeByMax.Checked) numericUpDownBreakByHours.Value = 1;
		}
	}
}
