using System;
using System.ComponentModel;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
	public partial class GamificationSettingRuleWithDifferentThresholdControl : BaseUserControl
	{
		private RuleSettingWithDifferentThreshold setting;
		public bool isValidValue = true;
		public RuleSettingWithDifferentThreshold CurrentSetting
		{
			get { return setting; }
			set
			{
				setting = value;
				updateControlValues();
			}
		}

		private void updateControlValues()
		{
			CheckBoxUseBadgeForAnsweredCalls.Checked = setting.AnsweredCallsBadgeEnabled;
			numericUpDownBronzeThresholdForAnsweredCalls.Value = setting.AnsweredCallsBronzeThreshold;
			numericUpDownSilverThresholdForAnsweredCalls.Value = setting.AnsweredCallsSilverThreshold;
			numericUpDownGoldThresholdForAnsweredCalls.Value = setting.AnsweredCallsGoldThreshold;
			
			checkBoxUseBadgeForAHT.Checked = setting.AHTBadgeEnabled;
			timeSpanTextBoxBronzeThresholdForAHT.SetInitialResolution(setting.AHTBronzeThreshold);
			timeSpanTextBoxSilverThresholdForAHT.SetInitialResolution(setting.AHTSilverThreshold);
			timeSpanTextBoxGoldThresholdForAHT.SetInitialResolution(setting.AHTGoldThreshold);

			checkBoxUseBadgeForAdherence.Checked = setting.AdherenceBadgeEnabled;
			doubleTextBoxBronzeThresholdForAdherence.DoubleValue = setting.AdherenceBronzeThreshold.Value * 100;
			doubleTextBoxSilverThresholdForAdherence.DoubleValue = setting.AdherenceSilverThreshold.Value * 100;
			doubleTextBoxGoldThresholdForAdherence.DoubleValue = setting.AdherenceGoldThreshold.Value * 100;
		}

		public GamificationSettingRuleWithDifferentThresholdControl()
		{
			InitializeComponent();
			SetTexts();
			timeSpanTextBoxBronzeThresholdForAHT.TimeSpanBoxWidth = 115;
			timeSpanTextBoxSilverThresholdForAHT.TimeSpanBoxWidth = 115;
			timeSpanTextBoxGoldThresholdForAHT.TimeSpanBoxWidth = 115;
			setting = new RuleSettingWithDifferentThreshold();
		}

		private void checkBoxUseBadgeForAnsweredCalls_CheckedChanged(object sender, EventArgs e)
		{
			numericUpDownBronzeThresholdForAnsweredCalls.Enabled = ((CheckBox)sender).Checked;
			numericUpDownSilverThresholdForAnsweredCalls.Enabled = ((CheckBox)sender).Checked;
			numericUpDownGoldThresholdForAnsweredCalls.Enabled = ((CheckBox)sender).Checked;

			setting.AnsweredCallsBadgeEnabled = ((CheckBox) sender).Checked;
		}

		private void checkBoxUseBadgeForAHT_CheckedChanged(object sender, EventArgs e)
		{
			timeSpanTextBoxBronzeThresholdForAHT.Enabled = ((CheckBox)sender).Checked;
			timeSpanTextBoxSilverThresholdForAHT.Enabled = ((CheckBox)sender).Checked;
			timeSpanTextBoxGoldThresholdForAHT.Enabled = ((CheckBox)sender).Checked;
			setting.AHTBadgeEnabled = ((CheckBox) sender).Checked;
		}

		private void checkBoxUseBadgeForAdherence_CheckedChanged(object sender, EventArgs e)
		{
			doubleTextBoxBronzeThresholdForAdherence.Enabled = ((CheckBox)sender).Checked;
			doubleTextBoxSilverThresholdForAdherence.Enabled = ((CheckBox)sender).Checked;
			doubleTextBoxGoldThresholdForAdherence.Enabled = ((CheckBox)sender).Checked;
			setting.AdherenceBadgeEnabled = ((CheckBox) sender).Checked;
		}

		private void numericUpDownThresholdsForAnsweredCalls_Validated(object sender, EventArgs e)
		{
			setting.AnsweredCallsGoldThreshold = (int) numericUpDownGoldThresholdForAnsweredCalls.Value;
			setting.AnsweredCallsSilverThreshold = (int) numericUpDownSilverThresholdForAnsweredCalls.Value;
			setting.AnsweredCallsBronzeThreshold = (int) numericUpDownBronzeThresholdForAnsweredCalls.Value;
		}

		private void numericUpDownGoldThresholdForAnsweredCalls_Validating(object sender, CancelEventArgs e)
		{
			if ((int) numericUpDownGoldThresholdForAnsweredCalls.Value >
			    (int) numericUpDownSilverThresholdForAnsweredCalls.Value)
			{
				isValidValue = true;
				return;
			}
			MessageBox.Show(Resources.GoldThresholdForAnsweredCallsValue);
			isValidValue = false;
		}

		private void numericUpDownSilverThresholdForAnsweredCalls_Validating(object sender, CancelEventArgs e)
		{
			if ((int) numericUpDownSilverThresholdForAnsweredCalls.Value >
			    (int) numericUpDownBronzeThresholdForAnsweredCalls.Value)
			{
				isValidValue = true;
				return;
			}
			MessageBox.Show(Resources.SilverThresholdForAnsweredCallsValue);
			isValidValue = false;
		}
		
		private void timeSpanTextBoxThresholdsForAHT_Validated(object sender, EventArgs e)
		{
			setting.AHTBronzeThreshold = timeSpanTextBoxBronzeThresholdForAHT.Value;
			setting.AHTSilverThreshold = timeSpanTextBoxSilverThresholdForAHT.Value;
			setting.AHTGoldThreshold = timeSpanTextBoxGoldThresholdForAHT.Value;
		}

		private void timeSpanTextBoxGoldThresholdsForAHT_Validating(object sender, CancelEventArgs e)
		{
			if (timeSpanTextBoxGoldThresholdForAHT.Value < timeSpanTextBoxSilverThresholdForAHT.Value)
			{
				isValidValue = true;
				return;
			}
			MessageBox.Show(Resources.TimeSpanTextBoxGoldThresholdForAHT);
			isValidValue = false;
		}

		private void timeSpanTextBoxSilverThresholdsForAHT_Validating(object sender, CancelEventArgs e)
		{
			if (timeSpanTextBoxSilverThresholdForAHT.Value < timeSpanTextBoxBronzeThresholdForAHT.Value)
			{
				isValidValue = true;
				return;
			}
			MessageBox.Show(Resources.TimeSpanTextBoxSilverThresholdsForAHT);
			isValidValue = false;
		}

		private void doubleTextBoxThresholdsForAdherence_Validated(object sender, EventArgs e)
		{
			setting.AdherenceBronzeThreshold = new Percent(doubleTextBoxBronzeThresholdForAdherence.DoubleValue / 100);
			setting.AdherenceSilverThreshold = new Percent(doubleTextBoxSilverThresholdForAdherence.DoubleValue / 100);
			setting.AdherenceGoldThreshold = new Percent(doubleTextBoxGoldThresholdForAdherence.DoubleValue / 100);
		}

		private void doubleTextBoxGoldThresholdsForAdherence_Validating(object sender, CancelEventArgs e)
		{
			if (doubleTextBoxGoldThresholdForAdherence.DoubleValue > doubleTextBoxSilverThresholdForAdherence.DoubleValue)
			{
				isValidValue = true;
				return;
			}
			MessageBox.Show(Resources.DoubleTextBoxGoldThresholdForAdherence);
			isValidValue = false;
		}

		private void doubleTextBoxSilverThresholdsForAdherence_Validating(object sender, CancelEventArgs e)
		{
			if (doubleTextBoxSilverThresholdForAdherence.DoubleValue > doubleTextBoxBronzeThresholdForAdherence.DoubleValue)
			{
				isValidValue = true;
				return;
			}
			MessageBox.Show(Resources.DoubleTextBoxGoldThresholdForAdherence);
			isValidValue = false;
		}
	}

	public class RuleSettingWithDifferentThreshold
	{
		public bool AnsweredCallsBadgeEnabled { get; set; }
		public bool AHTBadgeEnabled { get; set; }
		public bool AdherenceBadgeEnabled { get; set; }

		public int AnsweredCallsBronzeThreshold { get; set; }
		public int AnsweredCallsSilverThreshold { get; set; }
		public int AnsweredCallsGoldThreshold { get; set; }

		public TimeSpan AHTBronzeThreshold { get; set; }
		public TimeSpan AHTSilverThreshold { get; set; }
		public TimeSpan AHTGoldThreshold { get; set; }

		public Percent AdherenceBronzeThreshold { get; set; }
		public Percent AdherenceSilverThreshold { get; set; }
		public Percent AdherenceGoldThreshold { get; set; }
	}
}