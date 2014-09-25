using System;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Common;
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
		private AgentBadgeSettingsRepository _repository;
		private readonly IToggleManager _toggleManager;
		private IAgentBadgeRepository _agentBadgeRepository;

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
			return new TreeFamily(UserTexts.Resources.SystemSettings);
		}

		public string TreeNode()
		{
			return UserTexts.Resources.AgentBadgeSetting;
		}

		public void LoadControl()
		{
			var settings = _repository.LoadAll().FirstOrDefault();

			settings = settings ?? new AgentBadgeThresholdSettings
			{
				EnableBadge = false,
				AdherenceThreshold = new Percent(0.75),
				AnsweredCallsThreshold = 100,
				AHTThreshold = new TimeSpan(0, 5, 0),
				SilverToBronzeBadgeRate = 5,
				GoldToSilverBadgeRate = 2
			};

			checkBoxEnableBadge.Checked = settings.EnableBadge;
			doubleTextBoxThresholdForAdherence.DoubleValue = settings.AdherenceThreshold.Value * 100;
			checkAdherenceBadgeType.Checked = settings.AdherenceBadgeTypeSelected;
			timeSpanTextBoxThresholdForAHT.SetInitialResolution(settings.AHTThreshold);
			checkAHTBadgeType.Checked = settings.AHTBadgeTypeSelected;
			numericUpDownThresholdForAnsweredCalls.Value = settings.AnsweredCallsThreshold;
			checkAnsweredCallsBadgeType.Checked = settings.AnsweredCallsBadgeTypeSelected;
			numericUpDownSilverToBronzeBadgeRate.Value = settings.SilverToBronzeBadgeRate;
			numericUpDownGoldenToSilverBadgeRate.Value = settings.GoldToSilverBadgeRate;

			setControlsEnabled(settings.EnableBadge);
			timeSpanTextBoxThresholdForAHT.TimeSpanBoxWidth = 115;
		}

		public void SaveChanges()
		{
			var settings = _repository.LoadAll().FirstOrDefault() ?? new AgentBadgeThresholdSettings();
			settings.EnableBadge = checkBoxEnableBadge.Checked;
			settings.AdherenceThreshold = new Percent(doubleTextBoxThresholdForAdherence.DoubleValue / 100);
			settings.AdherenceBadgeTypeSelected = checkAdherenceBadgeType.Checked;
			settings.AHTThreshold = timeSpanTextBoxThresholdForAHT.Value;
			settings.AHTBadgeTypeSelected = checkAHTBadgeType.Checked;
			settings.AnsweredCallsThreshold = (int)numericUpDownThresholdForAnsweredCalls.Value;
			settings.AnsweredCallsBadgeTypeSelected = checkAnsweredCallsBadgeType.Checked;
			settings.GoldToSilverBadgeRate = (int)numericUpDownGoldenToSilverBadgeRate.Value;
			settings.SilverToBronzeBadgeRate = (int)numericUpDownSilverToBronzeBadgeRate.Value;

			_repository.Add(settings);
		}

		public void OnShow()
		{
		}

		public void SetUnitOfWork(IUnitOfWork value)
		{
			_unitOfWork = value;
			_repository = new AgentBadgeSettingsRepository(_unitOfWork);
			_agentBadgeRepository = new AgentBadgeRepository(_unitOfWork);
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
			var badgeEnabled = ((CheckBox) sender).Checked;
			setControlsEnabled(badgeEnabled);
		}

		private void setControlsEnabled(bool enabled)
		{
			checkAdherenceBadgeType.Enabled = enabled;
			checkAHTBadgeType.Enabled = enabled;
			checkAnsweredCallsBadgeType.Enabled = enabled;
		}

		private void checkAnsweredCallsBadgeType_CheckedChanged(object sender, EventArgs e)
		{
			var badgeTypeChecked = ((CheckBox) sender).Checked;
			numericUpDownThresholdForAnsweredCalls.Enabled = badgeTypeChecked;
			updateRateSettingsState();
		}

		private void checkAHTBadgeType_CheckedChanged(object sender, EventArgs e)
		{
			var badgeTypeChecked = ((CheckBox)sender).Checked;
			timeSpanTextBoxThresholdForAHT.Enabled = badgeTypeChecked;
			updateRateSettingsState();
		}

		private void checkAdherenceBadgeType_CheckedChanged(object sender, EventArgs e)
		{
			var badgeTypeChecked = ((CheckBox)sender).Checked;
			doubleTextBoxThresholdForAdherence.Enabled = badgeTypeChecked;
			updateRateSettingsState();
		}

		private void updateRateSettingsState()
		{
			var isAnyTypeSelected = doubleTextBoxThresholdForAdherence.Enabled || timeSpanTextBoxThresholdForAHT.Enabled ||
									numericUpDownThresholdForAnsweredCalls.Enabled;
			if (isAnyTypeSelected)
			{
				numericUpDownSilverToBronzeBadgeRate.Enabled = true;
				numericUpDownGoldenToSilverBadgeRate.Enabled = true;
			}
			else
			{
				numericUpDownSilverToBronzeBadgeRate.Enabled = false;
				numericUpDownGoldenToSilverBadgeRate.Enabled = false;
			}
		}

		private void reset_Click(object sender, EventArgs e)
		{
			DialogResult result = MessageBox.Show(Resources.ResetBadgesConfirm, Resources.ResetBadges, MessageBoxButtons.OKCancel);
			if (result == DialogResult.OK)
			{
				try
				{
					_agentBadgeRepository.ResetAgentBadges();
				}
				catch
				{
					MessageBox.Show(Resources.ResetBadgesFailed, Resources.ResetBadges, MessageBoxButtons.OK);
				}
			}
		}
	}
}
