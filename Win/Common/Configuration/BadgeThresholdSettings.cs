using System;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Win.Common.Configuration.Columns;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Common.Configuration
{

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public partial class BadgeThresholdSettings : BaseUserControl, ISettingPage
    {
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public BadgeThresholdSettings(IUnitOfWorkFactory unitOfWorkFactory)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			InitializeComponent();
		}

		public void InitializeDialogControl()
        {
            SetColors();
            SetTexts();
        }

        private void SetColors()
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
        }

		public void SaveChanges()
		{
			using (var uow = _unitOfWorkFactory.CurrentUnitOfWork())
			{
				var repository = new AgentBadgeSettingsRepository(uow);
				
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
    }
}
