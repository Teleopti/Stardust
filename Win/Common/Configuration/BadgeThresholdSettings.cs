using System;
using System.Linq;
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
            BackColor = ColorHelper.WizardBackgroundColor();
            tableLayoutPanelBody.BackColor = ColorHelper.WizardBackgroundColor();

            gradientPanelHeader.BackgroundColor = ColorHelper.OptionsDialogHeaderGradientBrush();
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
			return UserTexts.Resources.AgentBadgeThresholdSetting;
		}

		public void LoadControl()
		{
			var settings = _repository.LoadAll().FirstOrDefault();

			settings = settings ?? new AgentBadgeThresholdSettings
			{
				AdherenceThreshold = new Percent(0.75),
				AnsweredCallsThreshold = 100,
				AHTThreshold = new TimeSpan(0, 5, 0),
				SilverBadgeDaysThreshold = 5,
				GoldBadgeDaysThreshold = 10
			};

			doubleTextBoxThresholdForAdherence.DoubleValue = settings.AdherenceThreshold.Value;
			timeSpanTextBoxThresholdForAHT.SetInitialResolution(settings.AHTThreshold);
			numericUpDownThresholdForAnsweredCalls.Value = settings.AnsweredCallsThreshold;
			numericUpDownGoldenBadgeDaysThreshold.Value = settings.GoldBadgeDaysThreshold;
			numericUpDownSilverBadgeDaysThreshold.Value = settings.SilverBadgeDaysThreshold;
		}

		public void SaveChanges()
		{
			var settings = _repository.LoadAll().FirstOrDefault() ?? new AgentBadgeThresholdSettings();
			settings.AdherenceThreshold = new Percent(doubleTextBoxThresholdForAdherence.DoubleValue);
			settings.AHTThreshold = timeSpanTextBoxThresholdForAHT.Value;
			settings.AnsweredCallsThreshold = (int)numericUpDownThresholdForAnsweredCalls.Value;
			settings.GoldBadgeDaysThreshold = (int)numericUpDownGoldenBadgeDaysThreshold.Value;
			settings.SilverBadgeDaysThreshold = (int)numericUpDownSilverBadgeDaysThreshold.Value;

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
    }
}
