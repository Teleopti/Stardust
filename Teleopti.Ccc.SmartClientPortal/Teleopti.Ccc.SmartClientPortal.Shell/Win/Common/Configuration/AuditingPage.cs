using System;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Drawing;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories.Audit;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
	// copy/paste from AuditTrailPage
	// no mvc/p whatsoever here... 
	public partial class AuditingPage : BaseUserControl, ISettingPage
	{
		private IAuditSetting _auditSetting;
		private bool checkedWhenLoaded;

		public AuditingPage()
		{
			InitializeComponent();
		}

		private AuditSettingRepository Repository { get; set; }

		public void SetUnitOfWork(IUnitOfWork value)
		{
			Repository = new AuditSettingRepository(new ThisUnitOfWork(value));
		}

		public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
		{
		}

		public ViewType ViewType
		{
			get { return ViewType.AuditTrailSetting; }
		}

		public void InitializeDialogControl()
		{
			SetTexts();
		}

		public void LoadControl()
		{
			loadAuditSetting();
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

		private void loadAuditSetting()
		{
			checkBoxAdvIsRunning.CheckedChanged -= checkBoxAdvIsRunningCheckedChanged;
			_auditSetting = Repository.Read();
			checkedWhenLoaded = _auditSetting.IsScheduleEnabled;
			checkBoxAdvIsRunning.Checked = checkedWhenLoaded;
			setAuditTrailingStatus(checkBoxAdvIsRunning.Checked);

			checkBoxAdvIsRunning.CheckedChanged += checkBoxAdvIsRunningCheckedChanged;
		}

		public void SaveChanges()
		{
			if (checkedWhenLoaded != checkBoxAdvIsRunning.Checked)
			{
				if (checkBoxAdvIsRunning.Checked)
				{
					(_auditSetting as AuditSetting).TurnOnScheduleAuditing(Repository, UnitOfWorkFactory.Current.AuditSetting);
				}
				else
				{
					_auditSetting.TurnOffScheduleAuditing(UnitOfWorkFactory.Current.AuditSetting);
				}				
			}

			loadAuditSetting();
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
				autoLabelStatusText.Text = Resources.AuditingIsNotRunning;
			}
			setStatusPanelBackColor(backColor);
		}

		private void setStatusPanelBackColor(Color backColor)
		{
			var brushInfo = new BrushInfo(gradientPanelExtStatusText.BackgroundColor.PatternStyle, gradientPanelExtStatusText.BackgroundColor.ForeColor, backColor);
			gradientPanelExtStatusText.BackgroundColor = brushInfo;
		}

		private void checkBoxAdvIsRunningCheckedChanged(object sender, EventArgs e)
		{
			var areYouSureDialogResult = DialogResult.OK;
			var oldState = !checkBoxAdvIsRunning.Checked;
			if (checkBoxAdvIsRunning.Checked)
			{
				areYouSureDialogResult = MessageDialogs.ShowQuestion(this, Resources.AuditingTurnOnLong,
					Resources.AuditingTurnOnShort);
			}
			else
			{
				if (_auditSetting.IsScheduleEnabled)
				{
					areYouSureDialogResult = MessageDialogs.ShowQuestion(this, Resources.AuditTrailSettingOffQuestion,
						Resources.AuditTrailSetting);
				}
			}

			if (areYouSureDialogResult == DialogResult.No)
			{
				checkBoxAdvIsRunning.Checked = oldState;
			}
			else
			{
				setStatusPanelBackColor(Color.FromArgb(255, 224, 128));
			}
		}

		private void checkBoxAdvIsRunning_CheckStateChanged(object sender, EventArgs e)
		{

		}
	}
}
