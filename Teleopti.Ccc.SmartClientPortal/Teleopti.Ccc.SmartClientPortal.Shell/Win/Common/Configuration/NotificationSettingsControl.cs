using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
	public partial class NotificationSettingsControl : BaseUserControl, ISettingPage
	{
		private List<IOptionalColumn> _optionalColumnList;
		public OptionalColumnRepository Repository { get; private set; }
		public IUnitOfWork UnitOfWork { get; private set; }
		private SmsSettings _smsSettingsSetting;

		public IOptionalColumn SelectedOptionalColumn
		{
			get { return comboBoxOptionalColumns.SelectedItem as IOptionalColumn; }
		}

		public NotificationSettingsControl()
		{
			InitializeComponent();
			if (!DefinedLicenseDataFactory.GetLicenseActivator(UnitOfWorkFactory.Current.Name)
				.EnabledLicenseOptionPaths.Contains(DefinedLicenseOptionPaths.TeleoptiCccSmsLink))
			{
				//tableLayoutPanelSubHeader1.Hide();
				//tableLayoutPanelSubHeader2.Hide();
				//tableLayoutPanelSubHeader3.Hide();
				//labelSubHeader2.Hide();
				//radioButtonAdvEmail.Hide();
				//radioButtonAdvSMS.Hide();
				//autoLabel1.Hide();
				//autoLabel5.Hide();
				//comboBoxOptionalColumns.Hide();
				//textBoxEmailFrom.Hide();
				tableLayoutPanelSubHeader1.Visible = false;
				tableLayoutPanelSubHeader2.Visible = false;
				tableLayoutPanelSubHeader3.Visible = false;
				labelSubHeader2.Visible = false;
				tableLayoutPanel1.Visible = false;
				radioButtonAdvEmail.Visible = false;
				radioButtonAdvSMS.Visible = false;
				autoLabel1.Visible = false;
				autoLabel5.Visible = false;
				comboBoxOptionalColumns.Visible = false;
				textBoxEmailFrom.Visible = false;
				tableLayoutPanel1.Visible = false;
				tableLayoutPanel3.Visible = false;
			}
		}

		private void setColors()
		{
			BackColor = ColorHelper.WizardBackgroundColor();
			tableLayoutPanelBody.BackColor = ColorHelper.WizardBackgroundColor();

			gradientPanelHeader.BackColor = ColorHelper.OptionsDialogHeaderBackColor();
			labelHeader.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();

			tableLayoutPanelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			tableLayoutPanelSubHeader2.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			tableLayoutPanelSubHeader3.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			tableLayoutSubHeaderMobileNotification.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader1.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
			labelSubHeader2.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader2.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
			labelSubHeader3.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader3.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
			labelSubHeaderMobileNotificationSettings.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeaderMobileNotificationSettings.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();

		}

		private void loadNotificationSettings()
		{
			if (Disposing) return;
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_smsSettingsSetting = GlobalSettingDataRepository.DONT_USE_CTOR(uow).FindValueByKey("SmsSettings", new SmsSettings());
			}
			if (_optionalColumnList == null)
			{
				_optionalColumnList = new List<IOptionalColumn>();
				_optionalColumnList.AddRange(Repository.GetOptionalColumns<Person>());
			}

			comboBoxOptionalColumns.DisplayMember = "Name";
			comboBoxOptionalColumns.DataSource = _optionalColumnList;

			if (_smsSettingsSetting.NotificationSelection == NotificationType.Sms)
			{
				radioButtonAdvSMS.Checked = true;
				comboBoxOptionalColumns.SelectedIndex = getIndex(_optionalColumnList, _smsSettingsSetting.OptionalColumnId);
			}
			else
			{
				radioButtonAdvEmail.Checked = true;
				comboBoxOptionalColumns.SelectedIndex = -1;
				textBoxEmailFrom.Text = _smsSettingsSetting.EmailFrom;
			}

			checkBoxEnableMobileNotification.Checked = _smsSettingsSetting.IsMobileNotificationEnabled;
			radioButtonCheckChanged();
		}

		private static int getIndex(List<IOptionalColumn> optionalColumnList, Guid id)
		{
			foreach (var optionalColumn in optionalColumnList.Where(optionalColumn => optionalColumn.Id.Equals(id)))
			{
				return optionalColumnList.IndexOf(optionalColumn);
			}
			return -1;
		}

		public void InitializeDialogControl()
		{
			setColors();
			SetTexts();
		}

		public void LoadControl()
		{
			loadNotificationSettings();
		}

		public void SaveChanges()
		{
			if (_smsSettingsSetting != null)
			{
				if (comboBoxOptionalColumns.SelectedValue != null)
				{
					_smsSettingsSetting.NotificationSelection = NotificationType.Sms;
					_smsSettingsSetting.OptionalColumnId = ((OptionalColumn)comboBoxOptionalColumns.SelectedValue).Id.Value;
				}
				else if (!textBoxEmailFrom.Text.IsEmpty())
				{
					_smsSettingsSetting.NotificationSelection = NotificationType.Email;
					_smsSettingsSetting.EmailFrom = textBoxEmailFrom.Text;
				}

				_smsSettingsSetting.IsMobileNotificationEnabled = checkBoxEnableMobileNotification.Checked;
				using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					GlobalSettingDataRepository.DONT_USE_CTOR(uow).PersistSettingValue(_smsSettingsSetting);
					uow.PersistAll();
				}
			}
		}

		public void Unload()
		{
			_optionalColumnList = null;
		}

		public TreeFamily TreeFamily()
		{
			return new TreeFamily(Resources.SystemSettings);
		}

		public string TreeNode()
		{
			return Resources.NotificationSettings;
		}

		public void OnShow()
		{
			_optionalColumnList = null;
			loadNotificationSettings();
		}

		public void SetUnitOfWork(IUnitOfWork value)
		{
			UnitOfWork = value;
			Repository = new OptionalColumnRepository(UnitOfWork);
		}

		public void Persist()
		{
			if (_smsSettingsSetting != null)
			{
				using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					GlobalSettingDataRepository.DONT_USE_CTOR(uow).PersistSettingValue(_smsSettingsSetting);
				}
			}
		}

		public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
		{ }

		public ViewType ViewType
		{
			get { return ViewType.SmsSettings; }
		}

		private void radioButtonAdvSMS_CheckChanged(object sender, EventArgs e)
		{
			radioButtonCheckChanged();
		}

		private void radioButtonCheckChanged()
		{
			comboBoxOptionalColumns.Enabled = radioButtonAdvSMS.Checked;
			textBoxEmailFrom.Enabled = !radioButtonAdvSMS.Checked;

			clearSmsData(!radioButtonAdvSMS.Checked);
			setDefaultEmailData(radioButtonAdvSMS.Checked);
		}

		private void setDefaultEmailData(bool clear)
		{
			if (!clear) return;
			textBoxEmailFrom.Text = _smsSettingsSetting.EmailFrom;
		}

		private void clearSmsData(bool clear)
		{
			if (!clear) return;
			comboBoxOptionalColumns.SelectedIndex = -1;
		}

		private void textBoxEmailFrom_Leave(object sender, EventArgs e)
		{
			if (!IsValidEmail(textBoxEmailFrom.Text))
				textBoxEmailFrom.Text = _smsSettingsSetting.EmailFrom;
		}

		bool IsValidEmail(string email)
		{
			try
			{
				var addr = new System.Net.Mail.MailAddress(email);
				return addr.Address == email;
			}
			catch
			{
				return false;
			}
		}

	}
}
