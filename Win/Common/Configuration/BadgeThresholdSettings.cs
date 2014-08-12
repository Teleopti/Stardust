using System;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
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

		public BadgeThresholdSettings()
		{
			InitializeComponent();
		}

		public void InitializeDialogControl()
		{
			setColors();
			SetTexts();
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
			timeSpanTextBoxThresholdForAHT.SetInitialResolution(settings.AHTThreshold);
			numericUpDownThresholdForAnsweredCalls.Value = settings.AnsweredCallsThreshold;
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
			settings.AHTThreshold = timeSpanTextBoxThresholdForAHT.Value;
			settings.AnsweredCallsThreshold = (int)numericUpDownThresholdForAnsweredCalls.Value;
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
			doubleTextBoxThresholdForAdherence.Enabled = enabled;
			timeSpanTextBoxThresholdForAHT.Enabled = enabled;
			numericUpDownThresholdForAnsweredCalls.Enabled = enabled;
			numericUpDownSilverToBronzeBadgeRate.Enabled = enabled;
			numericUpDownGoldenToSilverBadgeRate.Enabled = enabled;
		}
	}
}
