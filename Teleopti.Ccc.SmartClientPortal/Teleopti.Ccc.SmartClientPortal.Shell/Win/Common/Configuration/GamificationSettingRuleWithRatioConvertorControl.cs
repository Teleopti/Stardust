using System;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
	public partial class GamificationSettingRuleWithRatioConvertorControl : BaseUserControl
	{
		private RuleSettingWithRatioConvertor setting;

		public RuleSettingWithRatioConvertor CurrentSetting
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
			numericUpDownThresholdForAnsweredCalls.Value = setting.AnsweredCallsThreshold;
			

			checkBoxUseBadgeForAHT.Checked = setting.AHTBadgeEnabled;
			timeSpanTextBoxThresholdForAHT.SetInitialResolution(setting.AHTThreshold);
			

			checkBoxUseBadgeForAdherence.Checked = setting.AdherenceBadgeEnabled;
			doubleTextBoxThresholdForAdherence.DoubleValue = setting.AdherenceThreshold.Value * 100;

			numericUpDownGoldToSilverBadgeRate.Value = setting.GoldToSilverBadgeRate;
			numericUpDownSilverToBronzeBadgeRate.Value = setting.SilverToBronzeBadgeRate;
		}


		public GamificationSettingRuleWithRatioConvertorControl()
		{
			InitializeComponent();
			SetTexts();
			timeSpanTextBoxThresholdForAHT.TimeSpanBoxWidth = 115;
			setting =  new RuleSettingWithRatioConvertor();
		}

		private void checkBoxUseBadgeForAnsweredCalls_CheckedChanged(object sender, EventArgs e)
		{
			numericUpDownThresholdForAnsweredCalls.Enabled = ((CheckBox)sender).Checked;
			updateRatioSettingsState();
			setting.AnsweredCallsBadgeEnabled = ((CheckBox) sender).Checked;
		}

		private void checkBoxUseBadgeForAHT_CheckedChanged(object sender, EventArgs e)
		{
			timeSpanTextBoxThresholdForAHT.Enabled = ((CheckBox)sender).Checked;

			updateRatioSettingsState();
			setting.AHTBadgeEnabled = ((CheckBox) sender).Checked;
		}

		private void checkBoxUseBadgeForAdherence_CheckedChanged(object sender, EventArgs e)
		{
			doubleTextBoxThresholdForAdherence.Enabled = ((CheckBox)sender).Checked;

			updateRatioSettingsState();
			setting.AdherenceBadgeEnabled = ((CheckBox) sender).Checked;
		}

		private void updateRatioSettingsState()
		{
			var isAnyTypeEnabled = (doubleTextBoxThresholdForAdherence.Enabled || timeSpanTextBoxThresholdForAHT.Enabled ||
								numericUpDownThresholdForAnsweredCalls.Enabled);
			numericUpDownGoldToSilverBadgeRate.Enabled = isAnyTypeEnabled;
			numericUpDownSilverToBronzeBadgeRate.Enabled = isAnyTypeEnabled;
		}

		private void numericUpDownThresholdsForAnsweredCalls_Validated(object sender, EventArgs e)
		{
			setting.AnsweredCallsThreshold = (int)numericUpDownThresholdForAnsweredCalls.Value;
		}

		private void timeSpanTextBoxThresholdsForAHT_Validated(object sender, EventArgs e)
		{
			setting.AHTThreshold = timeSpanTextBoxThresholdForAHT.Value;
		}

		private void doubleTextBoxThresholdsForAdherence_Validated(object sender, EventArgs e)
		{
			setting.AdherenceThreshold = new Percent(doubleTextBoxThresholdForAdherence.DoubleValue / 100);
		}

		private void numericUpDownBadgeRateConvertor_Validated(object sender, EventArgs e)
		{
			setting.GoldToSilverBadgeRate = (int)numericUpDownGoldToSilverBadgeRate.Value;
			setting.SilverToBronzeBadgeRate = (int)numericUpDownSilverToBronzeBadgeRate.Value;

		}
	}
	public class RuleSettingWithRatioConvertor
	{
		public bool AnsweredCallsBadgeEnabled { get; set; }
		public bool AHTBadgeEnabled { get; set; }
		public bool AdherenceBadgeEnabled { get; set; }

		public int AnsweredCallsThreshold { get; set; }

		public TimeSpan AHTThreshold { get; set; }

		public Percent AdherenceThreshold { get; set; }

		public int GoldToSilverBadgeRate { get; set; }
		public int SilverToBronzeBadgeRate { get; set; }
	}
}
