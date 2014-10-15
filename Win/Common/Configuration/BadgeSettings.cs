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
		private IUnitOfWork _unitOfWork;
		private IAgentBadgeSettingsRepository _repository;
		private readonly IToggleManager _toggleManager;
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
			doubleTextBoxThresholdForAdherence.DoubleValue = settings.AdherenceThreshold.Value * 100;
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

		public void SaveChanges()
		{
			var settings = _repository.GetSettings();
			settings.BadgeEnabled = checkBoxEnableBadge.Checked;
			settings.AdherenceThreshold = new Percent(doubleTextBoxThresholdForAdherence.DoubleValue / 100);
			settings.AdherenceBadgeEnabled = checkBoxAdherenceBadgeEnabled.Checked;
			settings.AHTThreshold = timeSpanTextBoxThresholdForAHT.Value;
			settings.AHTBadgeEnabled = checkBoxAHTBadgeEnabled.Checked;
			settings.AnsweredCallsThreshold = (int)numericUpDownThresholdForAnsweredCalls.Value;
			settings.AnsweredCallsBadgeEnabled = checkBoxAnsweredCallsBadgeEnabled.Checked;
			settings.GoldToSilverBadgeRate = (int)numericUpDownGoldenToSilverBadgeRate.Value;
			settings.SilverToBronzeBadgeRate = (int)numericUpDownSilverToBronzeBadgeRate.Value;

			_repository.PersistSettingValue(settings);
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
			checkBoxAdherenceBadgeEnabled.Enabled = enabled;
			checkBoxAHTBadgeEnabled.Enabled = enabled;
			checkBoxAnsweredCallsBadgeEnabled.Enabled = enabled;
		}

		private void checkBoxAnsweredCallsBadgeEnabled_CheckedChanged(object sender, EventArgs e)
		{
			numericUpDownThresholdForAnsweredCalls.Enabled = ((CheckBox)sender).Checked;
			updateRateSettingsState();
		}

		private void checkBoxAHTBadgeEnabled_CheckedChanged(object sender, EventArgs e)
		{
			timeSpanTextBoxThresholdForAHT.Enabled = ((CheckBox)sender).Checked;
			updateRateSettingsState();
		}

		private void checkBoxAdherenceBadgeEnabled_CheckedChanged(object sender, EventArgs e)
		{
			doubleTextBoxThresholdForAdherence.Enabled = ((CheckBox)sender).Checked;
			updateRateSettingsState();
		}

		private void updateRateSettingsState()
		{
			var isAnyTypeEnabled
				= checkBoxAHTBadgeEnabled.Checked
				|| checkBoxAdherenceBadgeEnabled.Checked
				|| checkBoxAnsweredCallsBadgeEnabled.Checked;

			numericUpDownSilverToBronzeBadgeRate.Enabled = isAnyTypeEnabled;
			numericUpDownGoldenToSilverBadgeRate.Enabled = isAnyTypeEnabled;
		}

		private void reset_Click(object sender, EventArgs e)
		{
			var result = ViewBase.ShowOkCancelMessage(Resources.ResetBadgesConfirm, Resources.ResetBadges);
			if (result != DialogResult.OK) return;
			try
			{
				_agentBadgeTransactionRepository.ResetAgentBadges();
			}
			catch
			{
				ViewBase.ShowErrorMessage(Resources.ResetBadgesFailed, Resources.ResetBadges);
			}
		}
	}
}
