using System.Drawing;
using Syncfusion.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories.Audit;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
	// copy/paste from AuditTrailPage
	// no mvc/p whatsoever here... 
	public partial class AuditingPage : BaseUserControl, ISettingPage
	{
		public AuditingPage()
		{
			InitializeComponent();
		}

		private AuditSettingRepository _repository;

		public void SetUnitOfWork(IUnitOfWork value)
		{
			_repository = new AuditSettingRepository(new ThisUnitOfWork(value));
		}

		public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
		{
		}

		public ViewType ViewType => ViewType.AuditTrailSetting;

		public void InitializeDialogControl()
		{
			SetTexts();
		}

		public void LoadControl()
		{
			setAuditTrailingStatus(_repository.Read().IsScheduleEnabled);
			setColors();
		}

		private void setColors()
		{
			BackColor = ColorHelper.WizardBackgroundColor();
			tableLayoutPanelBody.BackColor = ColorHelper.WizardBackgroundColor();

			gradientPanelHeader.BackColor = ColorHelper.OptionsDialogHeaderBackColor();
			labelHeader.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();

			tableLayoutPanelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
		   autoLabelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
		   autoLabelSubHeader1.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
		}

		public void SaveChanges()
		{
		}

		public void Persist()
		{ }

		public void Unload()
		{
		}

		public TreeFamily TreeFamily()
		{
			return new TreeFamily(Resources.SystemSettings, DefinedRaptorApplicationFunctionPaths.AuditTrailSettings);
		}

		public string TreeNode()
		{
			return Resources.AuditingPageName;
		}

		public void OnShow()
		{
		}

		private void setAuditTrailingStatus(bool status)
		{
			Color backColor;
			if (status)
			{
				backColor = Color.FromArgb(224, 255, 224);
				autoLabelStatusText.Text = Resources.AuditingIsRunning;
			}
			else
			{
				backColor = Color.FromArgb(255, 224, 224);
				autoLabelStatusText.Text = Resources.AuditingIsNotRunningPleaseContactTeleopti;
			}
			setStatusPanelBackColor(backColor);
		}

		private void setStatusPanelBackColor(Color backColor)
		{
			var brushInfo = new BrushInfo(gradientPanelExtStatusText.BackgroundColor.PatternStyle, gradientPanelExtStatusText.BackgroundColor.ForeColor, backColor);
			gradientPanelExtStatusText.BackgroundColor = brushInfo;
		}
	}
}
