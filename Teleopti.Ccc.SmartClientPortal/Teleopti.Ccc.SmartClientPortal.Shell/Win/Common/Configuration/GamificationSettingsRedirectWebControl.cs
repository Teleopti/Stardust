using System;
using System.Diagnostics;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
	public partial class GamificationSettingRedirectWebControl : BaseUserControl, ISettingPage
	{
		private readonly IConfigReader _configReader;
		private readonly string _webLinkGamification;

		public GamificationSettingRedirectWebControl(IConfigReader configReader)
		{
			_configReader = configReader;
			InitializeComponent();
			_webLinkGamification = buildWfmUri("WFM/#/gamification").ToString();

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

			gradientPanelHeader.BackColor = ColorHelper.OptionsDialogHeaderBackColor();
			labelHeader.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();

			tableLayoutPanelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			autoLabelSubHeader1.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
		}

		public void LoadControl()
		{
			Process.Start(_webLinkGamification);
		}

		
		public void SaveChanges()
		{}

		public void Unload()
		{}

		public TreeFamily TreeFamily()
		{
			return new TreeFamily(Resources.Gamification);
		}

		public string TreeNode()
		{
			return Resources.GamificationSettings;
		}

		public void OnShow()
		{}

		public void SetUnitOfWork(IUnitOfWork value)
		{ }

		public void Persist()
		{}

		public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
		{
			throw new NotImplementedException();
		}

		public ViewType ViewType
		{
			get { return ViewType.SystemSetting; }
		}

		private Uri buildWfmUri(string relativePath)
		{
			var wfmPath = _configReader.AppConfig("FeatureToggle");
			return new Uri($"{wfmPath}{relativePath}");
		}

		private void linkGamificationWebRedirect_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			Process.Start(_webLinkGamification);
		}
	}
}
