using System;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
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
		private IUnitOfWork _unitOfWork;
		private IAgentBadgeSettingsRepository _repository;
		private IAgentBadgeTransactionRepository _agentBadgeTransactionRepository;

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
			if (!_toggleManager.IsEnabled(Toggles.Gamification_NewBadgeCalculation_31185))
			{
				checkBoxCalculateBadgeWithRank.Hide();
				toggleDifferentLevelBadges(false);
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
			var settings = _repository.GetSettings();

			checkBoxEnableBadge.Checked = settings.BadgeEnabled;
			checkBoxCalculateBadgeWithRank.Checked = settings.CalculateBadgeWithRank;

			checkBoxAdherenceBadgeEnabled.Checked = settings.AdherenceBadgeEnabled;
			doubleTextBoxThresholdForAdherence.DoubleValue = settings.AdherenceThreshold.Value*100;
			doubleTextBoxBronzeThresholdForAdherence.DoubleValue = settings.AdherenceBronzeThreshold.Value*100;
			doubleTextBoxSilverThresholdForAdherence.DoubleValue = settings.AdherenceSilverThreshold.Value * 100;
			doubleTextBoxGoldThresholdForAdherence.DoubleValue = settings.AdherenceGoldThreshold.Value * 100;

			checkBoxAHTBadgeEnabled.Checked = settings.AHTBadgeEnabled;
			timeSpanTextBoxThresholdForAHT.SetInitialResolution(settings.AHTThreshold);
			timeSpanTextBoxBronzeThresholdForAHT.SetInitialResolution(settings.AHTBronzeThreshold);
			timeSpanTextBoxSilverThresholdForAHT.SetInitialResolution(settings.AHTSilverThreshold);
			timeSpanTextBoxGoldThresholdForAHT.SetInitialResolution(settings.AHTGoldThreshold);

			checkBoxAnsweredCallsBadgeEnabled.Checked = settings.AnsweredCallsBadgeEnabled;
			numericUpDownThresholdForAnsweredCalls.Value = settings.AnsweredCallsThreshold;
			numericUpDownBronzeThresholdForAnsweredCalls.Value = settings.AnsweredCallsBronzeThreshold;
			numericUpDownSilverThresholdForAnsweredCalls.Value = settings.AnsweredCallsSilverThreshold;
			numericUpDownGoldThresholdForAnsweredCalls.Value = settings.AnsweredCallsGoldThreshold;

			numericUpDownSilverToBronzeBadgeRate.Value = settings.SilverToBronzeBadgeRate;
			numericUpDownGoldToSilverBadgeRate.Value = settings.GoldToSilverBadgeRate;

			setControlsEnabled(settings.BadgeEnabled);
			toggleDifferentLevelBadges(settings.CalculateBadgeWithRank);
			timeSpanTextBoxThresholdForAHT.TimeSpanBoxWidth = 115;
			timeSpanTextBoxBronzeThresholdForAHT.TimeSpanBoxWidth = 115;
			timeSpanTextBoxSilverThresholdForAHT.TimeSpanBoxWidth = 115;
			timeSpanTextBoxGoldThresholdForAHT.TimeSpanBoxWidth = 115;
		}

		public void SaveChanges()
		{
			if (!allSettingIsValid()) return;

			var settings = _repository.GetSettings();
			settings.BadgeEnabled = checkBoxEnableBadge.Checked;

			settings.CalculateBadgeWithRank = checkBoxCalculateBadgeWithRank.Checked;

			settings.AdherenceBadgeEnabled = checkBoxAdherenceBadgeEnabled.Checked;
			settings.AdherenceThreshold = new Percent(doubleTextBoxThresholdForAdherence.DoubleValue / 100);
			settings.AdherenceBronzeThreshold = new Percent(doubleTextBoxBronzeThresholdForAdherence.DoubleValue / 100);
			settings.AdherenceSilverThreshold = new Percent(doubleTextBoxSilverThresholdForAdherence.DoubleValue / 100);
			settings.AdherenceGoldThreshold = new Percent(doubleTextBoxGoldThresholdForAdherence.DoubleValue / 100);

			settings.AHTBadgeEnabled = checkBoxAHTBadgeEnabled.Checked;
			settings.AHTThreshold = timeSpanTextBoxThresholdForAHT.Value;
			settings.AHTBronzeThreshold = timeSpanTextBoxBronzeThresholdForAHT.Value;
			settings.AHTSilverThreshold = timeSpanTextBoxSilverThresholdForAHT.Value;
			settings.AHTGoldThreshold = timeSpanTextBoxGoldThresholdForAHT.Value;

			settings.AnsweredCallsBadgeEnabled = checkBoxAnsweredCallsBadgeEnabled.Checked;
			settings.AnsweredCallsThreshold = (int)numericUpDownThresholdForAnsweredCalls.Value;
			settings.AnsweredCallsBronzeThreshold = (int)numericUpDownBronzeThresholdForAnsweredCalls.Value;
			settings.AnsweredCallsSilverThreshold = (int)numericUpDownSilverThresholdForAnsweredCalls.Value;
			settings.AnsweredCallsGoldThreshold = (int)numericUpDownGoldThresholdForAnsweredCalls.Value;
			
			settings.GoldToSilverBadgeRate = (int) numericUpDownGoldToSilverBadgeRate.Value;
			settings.SilverToBronzeBadgeRate = (int) numericUpDownSilverToBronzeBadgeRate.Value;

			_repository.PersistSettingValue(settings);
		}

		private bool allSettingIsValid()
		{
			if (checkBoxEnableBadge.Checked
				&& !checkBoxAHTBadgeEnabled.Checked
				&& !checkBoxAdherenceBadgeEnabled.Checked
				&& !checkBoxAnsweredCallsBadgeEnabled.Checked)
			{
				ViewBase.ShowErrorMessage(Resources.NeedEnableAtLeastOneBadgeType, Resources.AgentBadgeSetting);
				checkBoxEnableBadge.Focus();
				return false;
			}

			if (checkBoxCalculateBadgeWithRank.Checked)
			{
				if (checkBoxAnsweredCallsBadgeEnabled.Checked)
				{
					var bronzeThresholdForAnsweredCalls = numericUpDownBronzeThresholdForAnsweredCalls.Value;
					var silverThresholdForAnsweredCalls = numericUpDownSilverThresholdForAnsweredCalls.Value;
					var goldThresholdForAnsweredCalls = numericUpDownGoldThresholdForAnsweredCalls.Value;
					if (bronzeThresholdForAnsweredCalls >= silverThresholdForAnsweredCalls)
					{
						ViewBase.ShowErrorMessage(Resources.AnsweredCallsBronzeThresholdShouldLessThanSilverThreshold,
							Resources.AgentBadgeSetting);
						numericUpDownBronzeThresholdForAnsweredCalls.Focus();
						return false;
					}

					if (silverThresholdForAnsweredCalls >= goldThresholdForAnsweredCalls)
					{
						ViewBase.ShowErrorMessage(Resources.AnsweredCallsSilverThresholdShouldLessThanGoldThreshold,
							Resources.AgentBadgeSetting);
						numericUpDownSilverThresholdForAnsweredCalls.Focus();
						return false;
					}
				}

				if (checkBoxAHTBadgeEnabled.Checked)
				{
					var bronzeThresholdForAHT = timeSpanTextBoxBronzeThresholdForAHT.Value;
					var silverThresholdForAHT = timeSpanTextBoxSilverThresholdForAHT.Value;
					var goldThresholdForAHT = timeSpanTextBoxGoldThresholdForAHT.Value;
					if (bronzeThresholdForAHT <= silverThresholdForAHT)
					{
						ViewBase.ShowErrorMessage(Resources.AHTBronzeThresholdShouldLessThanSilverThreshold,
							Resources.AgentBadgeSetting);
						timeSpanTextBoxBronzeThresholdForAHT.Focus();
						return false;
					}

					if (silverThresholdForAHT <= goldThresholdForAHT)
					{
						ViewBase.ShowErrorMessage(Resources.AHTSilverThresholdShouldLessThanGoldThreshold,
							Resources.AgentBadgeSetting);
						timeSpanTextBoxSilverThresholdForAHT.Focus();
						return false;
					}
				}

				if (checkBoxAdherenceBadgeEnabled.Checked)
				{
					var bronzeThresholdForAdherence = doubleTextBoxBronzeThresholdForAdherence.DoubleValue;
					var silverThresholdForAdherence = doubleTextBoxSilverThresholdForAdherence.DoubleValue;
					var goldThresholdForAdherence = doubleTextBoxGoldThresholdForAdherence.DoubleValue;
					if (bronzeThresholdForAdherence >= silverThresholdForAdherence)
					{
						ViewBase.ShowErrorMessage(Resources.AdherenceBronzeThresholdShouldGreaterThanSilverThreshold,
							Resources.AgentBadgeSetting);
						doubleTextBoxBronzeThresholdForAdherence.Focus();
						return false;
					}

					if (silverThresholdForAdherence >= goldThresholdForAdherence)
					{
						ViewBase.ShowErrorMessage(Resources.AdherenceSilverThresholdShouldGreaterThanGoldThreshold,
							Resources.AgentBadgeSetting);
						doubleTextBoxSilverThresholdForAdherence.Focus();
						return false;
					}
				}
			}

			return true;
		}

		public void OnShow()
		{
		}

		public void SetUnitOfWork(IUnitOfWork value)
		{
			_unitOfWork = value;
			_repository = new AgentBadgeSettingsRepository(_unitOfWork);
			_agentBadgeTransactionRepository = new AgentBadgeTransactionRepository(_unitOfWork);
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
			checkBoxCalculateBadgeWithRank.Enabled = enabled;

			checkBoxAdherenceBadgeEnabled.Enabled = enabled;
			checkBoxAHTBadgeEnabled.Enabled = enabled;
			checkBoxAnsweredCallsBadgeEnabled.Enabled = enabled;

			updateSettingsState();
		}

		private void checkBoxAnsweredCallsBadgeEnabled_CheckedChanged(object sender, EventArgs e)
		{
			numericUpDownThresholdForAnsweredCalls.Enabled = ((CheckBox)sender).Checked;
			numericUpDownBronzeThresholdForAnsweredCalls.Enabled = ((CheckBox)sender).Checked;
			numericUpDownSilverThresholdForAnsweredCalls.Enabled = ((CheckBox)sender).Checked;
			numericUpDownGoldThresholdForAnsweredCalls.Enabled = ((CheckBox)sender).Checked;
			updateSettingsState();
		}

		private void checkBoxAHTBadgeEnabled_CheckedChanged(object sender, EventArgs e)
		{
			timeSpanTextBoxThresholdForAHT.Enabled = ((CheckBox)sender).Checked;
			timeSpanTextBoxBronzeThresholdForAHT.Enabled = ((CheckBox)sender).Checked;
			timeSpanTextBoxSilverThresholdForAHT.Enabled = ((CheckBox)sender).Checked;
			timeSpanTextBoxGoldThresholdForAHT.Enabled = ((CheckBox)sender).Checked;
			updateSettingsState();
		}

		private void checkBoxAdherenceBadgeEnabled_CheckedChanged(object sender, EventArgs e)
		{
			doubleTextBoxThresholdForAdherence.Enabled = ((CheckBox)sender).Checked;
			doubleTextBoxBronzeThresholdForAdherence.Enabled = ((CheckBox)sender).Checked;
			doubleTextBoxSilverThresholdForAdherence.Enabled = ((CheckBox)sender).Checked;
			doubleTextBoxGoldThresholdForAdherence.Enabled = ((CheckBox)sender).Checked;
			updateSettingsState();
		}

		private void updateSettingsState()
		{
			var answeredCallsBadgeEnabled = checkBoxAnsweredCallsBadgeEnabled.Enabled &&
											checkBoxAnsweredCallsBadgeEnabled.Checked;
			numericUpDownThresholdForAnsweredCalls.Enabled = answeredCallsBadgeEnabled;
			numericUpDownBronzeThresholdForAnsweredCalls.Enabled = answeredCallsBadgeEnabled;
			numericUpDownSilverThresholdForAnsweredCalls.Enabled = answeredCallsBadgeEnabled;
			numericUpDownGoldThresholdForAnsweredCalls.Enabled = answeredCallsBadgeEnabled;

			var ahtBadgeEnabled = checkBoxAHTBadgeEnabled.Enabled && checkBoxAHTBadgeEnabled.Checked;
			timeSpanTextBoxThresholdForAHT.Enabled = ahtBadgeEnabled;
			timeSpanTextBoxBronzeThresholdForAHT.Enabled = ahtBadgeEnabled;
			timeSpanTextBoxSilverThresholdForAHT.Enabled = ahtBadgeEnabled;
			timeSpanTextBoxGoldThresholdForAHT.Enabled = ahtBadgeEnabled;

			var adherenceBadgeEnabled = checkBoxAdherenceBadgeEnabled.Enabled && checkBoxAdherenceBadgeEnabled.Checked;
			doubleTextBoxThresholdForAdherence.Enabled = adherenceBadgeEnabled;
			doubleTextBoxBronzeThresholdForAdherence.Enabled = adherenceBadgeEnabled;
			doubleTextBoxSilverThresholdForAdherence.Enabled = adherenceBadgeEnabled;
			doubleTextBoxGoldThresholdForAdherence.Enabled = adherenceBadgeEnabled;

			var isAnyTypeEnabled
				= (checkBoxEnableBadge.Checked) &&
				(checkBoxAHTBadgeEnabled.Checked
				|| checkBoxAdherenceBadgeEnabled.Checked
				|| checkBoxAnsweredCallsBadgeEnabled.Checked);

			numericUpDownSilverToBronzeBadgeRate.Enabled = isAnyTypeEnabled;
			numericUpDownGoldToSilverBadgeRate.Enabled = isAnyTypeEnabled;
		}

		private void reset_Click(object sender, EventArgs e)
		{
			var result = ViewBase.ShowOkCancelMessage(Resources.ResetBadgesConfirm, Resources.ResetBadges);
			if (result != DialogResult.OK) return;
			try
			{
				_agentBadgeTransactionRepository.ResetAgentBadges();
			}
			catch (Exception)
			{
				ViewBase.ShowErrorMessage(Resources.ResetBadgesFailed, Resources.ResetBadges);
			}
		}

		private void toggleRowDisplay(Control control, bool display)
		{
			var rowIndex = tableLayoutPanelBody.GetRow(control);
			if (rowIndex > 0)
			{
				tableLayoutPanelBody.RowStyles[rowIndex].Height = display ? 35 : 0;
			}
		}

		private void toggleDifferentLevelBadges(bool differentLevelBadgesEnabled)
		{
			labelSetThresholdForAnsweredCalls.Visible = !differentLevelBadgesEnabled;
			numericUpDownThresholdForAnsweredCalls.Visible = !differentLevelBadgesEnabled;
			toggleRowDisplay(numericUpDownThresholdForAnsweredCalls, !differentLevelBadgesEnabled);

			labelSetBronzeThresholdForAnsweredCalls.Visible = differentLevelBadgesEnabled;
			labelSetSilverThresholdForAnsweredCalls.Visible = differentLevelBadgesEnabled;
			labelSetGoldThresholdForAnsweredCalls.Visible = differentLevelBadgesEnabled;
			numericUpDownBronzeThresholdForAnsweredCalls.Visible = differentLevelBadgesEnabled;
			numericUpDownSilverThresholdForAnsweredCalls.Visible = differentLevelBadgesEnabled;
			numericUpDownGoldThresholdForAnsweredCalls.Visible = differentLevelBadgesEnabled;
			toggleRowDisplay(numericUpDownBronzeThresholdForAnsweredCalls, differentLevelBadgesEnabled);
			toggleRowDisplay(numericUpDownSilverThresholdForAnsweredCalls, differentLevelBadgesEnabled);
			toggleRowDisplay(numericUpDownGoldThresholdForAnsweredCalls, differentLevelBadgesEnabled);


			labelSetThresholdForAHT.Visible = !differentLevelBadgesEnabled;
			timeSpanTextBoxThresholdForAHT.Visible = !differentLevelBadgesEnabled;
			toggleRowDisplay(timeSpanTextBoxThresholdForAHT, !differentLevelBadgesEnabled);

			labelSetBronzeThresholdForAHT.Visible = differentLevelBadgesEnabled;
			labelSetSilverThresholdForAHT.Visible = differentLevelBadgesEnabled;
			labelSetGoldThresholdForAHT.Visible = differentLevelBadgesEnabled;
			timeSpanTextBoxBronzeThresholdForAHT.Visible = differentLevelBadgesEnabled;
			timeSpanTextBoxSilverThresholdForAHT.Visible = differentLevelBadgesEnabled;
			timeSpanTextBoxGoldThresholdForAHT.Visible = differentLevelBadgesEnabled;
			toggleRowDisplay(timeSpanTextBoxBronzeThresholdForAHT, differentLevelBadgesEnabled);
			toggleRowDisplay(timeSpanTextBoxSilverThresholdForAHT, differentLevelBadgesEnabled);
			toggleRowDisplay(timeSpanTextBoxGoldThresholdForAHT, differentLevelBadgesEnabled);

			labelSetThresholdForAdherence.Visible = !differentLevelBadgesEnabled;
			doubleTextBoxThresholdForAdherence.Visible = !differentLevelBadgesEnabled;
			toggleRowDisplay(doubleTextBoxThresholdForAdherence, !differentLevelBadgesEnabled);

			labelSetBronzeThresholdForAdherence.Visible = differentLevelBadgesEnabled;
			labelSetSilverThresholdForAdherence.Visible = differentLevelBadgesEnabled;
			labelSetGoldThresholdForAdherence.Visible = differentLevelBadgesEnabled;
			doubleTextBoxBronzeThresholdForAdherence.Visible = differentLevelBadgesEnabled;
			doubleTextBoxSilverThresholdForAdherence.Visible = differentLevelBadgesEnabled;
			doubleTextBoxGoldThresholdForAdherence.Visible = differentLevelBadgesEnabled;
			toggleRowDisplay(doubleTextBoxBronzeThresholdForAdherence, differentLevelBadgesEnabled);
			toggleRowDisplay(doubleTextBoxSilverThresholdForAdherence, differentLevelBadgesEnabled);
			toggleRowDisplay(doubleTextBoxGoldThresholdForAdherence, differentLevelBadgesEnabled);

			labelSplitter4.Visible = !differentLevelBadgesEnabled;
			labelOneSilverBadgeEqualsBronzeBadgeCount.Visible = !differentLevelBadgesEnabled;
			labelOneGoldBadgeEqualsSilverBadgeCount.Visible = !differentLevelBadgesEnabled;
			numericUpDownSilverToBronzeBadgeRate.Visible = !differentLevelBadgesEnabled;
			numericUpDownGoldToSilverBadgeRate.Visible = !differentLevelBadgesEnabled;
			toggleRowDisplay(labelSplitter4, !differentLevelBadgesEnabled);
			toggleRowDisplay(labelOneSilverBadgeEqualsBronzeBadgeCount, !differentLevelBadgesEnabled);
			toggleRowDisplay(labelOneGoldBadgeEqualsSilverBadgeCount, !differentLevelBadgesEnabled);
		}

		private void checkBoxEnableDifferentLevelBadges_CheckedChanged(object sender, EventArgs e)
		{
			var differentLevelBadgesEnabled = ((CheckBox) sender).Checked;
			toggleDifferentLevelBadges(differentLevelBadgesEnabled);
		}
	}
}
