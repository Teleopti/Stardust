using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Common.Configuration
{
	public partial class GamificationSettingRuleWithRatioConvertorControl : BaseUserControl
	{
		public GamificationSettingRuleWithRatioConvertorControl()
		{
			InitializeComponent();
			SetTexts();
			timeSpanTextBoxThresholdForAHT.TimeSpanBoxWidth = 115;
		}

		private void checkBoxUseBadgeForAnsweredCalls_CheckedChanged(object sender, EventArgs e)
		{
			numericUpDownThresholdForAnsweredCalls.Enabled = ((CheckBox)sender).Checked;
			updateRatioSettingsState();
		}

		private void checkBoxUseBadgeForAHT_CheckedChanged(object sender, EventArgs e)
		{
			timeSpanTextBoxThresholdForAHT.Enabled = ((CheckBox)sender).Checked;

			updateRatioSettingsState();
		}

		private void checkBoxUseBadgeForAdherence_CheckedChanged(object sender, EventArgs e)
		{
			doubleTextBoxThresholdForAdherence.Enabled = ((CheckBox)sender).Checked;

			updateRatioSettingsState();
		}

		private void updateRatioSettingsState()
		{
			var isAnyTypeEnabled = (doubleTextBoxThresholdForAdherence.Enabled || timeSpanTextBoxThresholdForAHT.Enabled ||
								numericUpDownThresholdForAnsweredCalls.Enabled);
			numericUpDownGoldToSilverBadgeRate.Enabled = isAnyTypeEnabled;
			numericUpDownSilverToBronzeBadgeRate.Enabled = isAnyTypeEnabled;
		}
	}
}
