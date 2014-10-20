﻿using System;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Common.Configuration
{

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public partial class BadgeThresholdSettings : BaseUserControl, ISettingPage
	{
		private readonly IToggleManager _toggleManager;

		public BadgeThresholdSettings(IToggleManager toggleManager)
		{
			_toggleManager = toggleManager;
			InitializeComponent();
		}

		public void InitializeDialogControl()
		{
			setColors();
			SetTexts();
			if (!_toggleManager.IsEnabled(Toggles.Portal_ResetBadges_30544))
			{
				reset.Hide();
			}
		}

		private void setColors()
		{
			tableLayoutPanelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader1.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();

			gradientPanelHeader.BackColor = ColorHelper.OptionsDialogHeaderBackColor();
			labelHeader.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();
		}

		public void Unload()
		{
		}

		public TreeFamily TreeFamily()
		{
			return new TreeFamily(Resources.SystemSettings);
		}

		public string TreeNode()
		{
			return Resources.AgentBadgeSetting;
		}

		public void LoadControl()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var settingRepository = new AgentBadgeSettingsRepository(uow);
				var settings = settingRepository.GetSettings();

				checkBoxEnableBadge.Checked = settings.BadgeEnabled;
				doubleTextBoxThresholdForAdherence.DoubleValue = settings.AdherenceThreshold.Value*100;
				checkBoxAdherenceBadgeEnabled.Checked = settings.AdherenceBadgeEnabled;
				timeSpanTextBoxThresholdForAHT.SetInitialResolution(settings.AHTThreshold);
				checkBoxAHTBadgeEnabled.Checked = settings.AHTBadgeEnabled;
				numericUpDownThresholdForAnsweredCalls.Value = settings.AnsweredCallsThreshold;
				checkBoxAnsweredCallsBadgeEnabled.Checked = settings.AnsweredCallsBadgeEnabled;
				numericUpDownSilverToBronzeBadgeRate.Value = settings.SilverToBronzeBadgeRate;
				numericUpDownGoldenToSilverBadgeRate.Value = settings.GoldToSilverBadgeRate;

				setControlsEnabled(settings.BadgeEnabled);
				timeSpanTextBoxThresholdForAHT.TimeSpanBoxWidth = 115;
			}
		}

		public void SaveChanges()
		{
			if (checkBoxEnableBadge.Checked
				&& !checkBoxAHTBadgeEnabled.Checked
				&& !checkBoxAdherenceBadgeEnabled.Checked
			    && !checkBoxAnsweredCallsBadgeEnabled.Checked)
			{
				ViewBase.ShowErrorMessage(Resources.NeedEnableAtLeastOneBadgeType, Resources.AgentBadgeSetting);
				return;
			}

			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var settingRepository = new AgentBadgeSettingsRepository(uow);
				var settings = settingRepository.GetSettings();
				settings.BadgeEnabled = checkBoxEnableBadge.Checked;
				settings.AdherenceThreshold = new Percent(doubleTextBoxThresholdForAdherence.DoubleValue/100);
				settings.AdherenceBadgeEnabled = checkBoxAdherenceBadgeEnabled.Checked;
				settings.AHTThreshold = timeSpanTextBoxThresholdForAHT.Value;
				settings.AHTBadgeEnabled = checkBoxAHTBadgeEnabled.Checked;
				settings.AnsweredCallsThreshold = (int) numericUpDownThresholdForAnsweredCalls.Value;
				settings.AnsweredCallsBadgeEnabled = checkBoxAnsweredCallsBadgeEnabled.Checked;
				settings.GoldToSilverBadgeRate = (int) numericUpDownGoldenToSilverBadgeRate.Value;
				settings.SilverToBronzeBadgeRate = (int) numericUpDownSilverToBronzeBadgeRate.Value;

				settingRepository.PersistSettingValue(settings);
			}
		}

		public void OnShow()
		{
		}

		public void SetUnitOfWork(IUnitOfWork value)
		{
		}

		public void Persist()
		{
		}

		public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
		{
			throw new NotImplementedException();
		}

		public ViewType ViewType { get; private set; }

		private void checkBoxEnableBadge_CheckedChanged(object sender, EventArgs e)
		{
			setControlsEnabled(((CheckBox)sender).Checked);
		}

		private void setControlsEnabled(bool enabled)
		{
			checkBoxAdherenceBadgeEnabled.Enabled = enabled;
			checkBoxAHTBadgeEnabled.Enabled = enabled;
			checkBoxAnsweredCallsBadgeEnabled.Enabled = enabled;

			updateSettingsState();
		}

		private void checkBoxAnsweredCallsBadgeEnabled_CheckedChanged(object sender, EventArgs e)
		{
			numericUpDownThresholdForAnsweredCalls.Enabled = ((CheckBox)sender).Checked;
			updateSettingsState();
		}

		private void checkBoxAHTBadgeEnabled_CheckedChanged(object sender, EventArgs e)
		{
			timeSpanTextBoxThresholdForAHT.Enabled = ((CheckBox)sender).Checked;
			updateSettingsState();
		}

		private void checkBoxAdherenceBadgeEnabled_CheckedChanged(object sender, EventArgs e)
		{
			doubleTextBoxThresholdForAdherence.Enabled = ((CheckBox)sender).Checked;
			updateSettingsState();
		}

		private void updateSettingsState()
		{
			numericUpDownThresholdForAnsweredCalls.Enabled = checkBoxAnsweredCallsBadgeEnabled.Enabled &&
															 checkBoxAnsweredCallsBadgeEnabled.Checked;
			timeSpanTextBoxThresholdForAHT.Enabled = checkBoxAHTBadgeEnabled.Enabled && checkBoxAHTBadgeEnabled.Checked;

			doubleTextBoxThresholdForAdherence.Enabled = checkBoxAdherenceBadgeEnabled.Enabled &&
														 checkBoxAdherenceBadgeEnabled.Checked;

			var isAnyTypeEnabled
				= (checkBoxEnableBadge.Checked) && (checkBoxAHTBadgeEnabled.Checked
				|| checkBoxAdherenceBadgeEnabled.Checked
				|| checkBoxAnsweredCallsBadgeEnabled.Checked);

			numericUpDownSilverToBronzeBadgeRate.Enabled = isAnyTypeEnabled;
			numericUpDownGoldenToSilverBadgeRate.Enabled = isAnyTypeEnabled;
		}

		private void reset_Click(object sender, EventArgs e)
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var result = ViewBase.ShowOkCancelMessage(Resources.ResetBadgesConfirm, Resources.ResetBadges);
				if (result != DialogResult.OK) return;
				try
				{
					var agentBadgeTransactionRepository = new AgentBadgeTransactionRepository(uow);
					agentBadgeTransactionRepository.ResetAgentBadges();
				}
				catch (Exception ex)
				{
					ViewBase.ShowWarningMessage("unit work: " + uow, Resources.ResetBadges);
					ViewBase.ShowWarningMessage(ex.Message, Resources.ResetBadges);
					ViewBase.ShowWarningMessage(ex.StackTrace, Resources.ResetBadges);
					ViewBase.ShowErrorMessage(Resources.ResetBadgesFailed, Resources.ResetBadges);
				}
			}
		}
	}
}
