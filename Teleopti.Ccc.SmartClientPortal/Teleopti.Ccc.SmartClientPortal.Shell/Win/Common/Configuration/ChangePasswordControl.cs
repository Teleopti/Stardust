using System;
using Autofac;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
	public partial class ChangePasswordControl : BaseUserControl, ISettingPage, IChangePasswordView, ICheckBeforeClosing
	{
		private readonly IComponentContext _container;
		private ChangePasswordPresenter _presenter;
		private bool _canClose;

		public ChangePasswordControl(IComponentContext container)
		{
			_container = container;
			InitializeComponent();
		}

		public void InitializeDialogControl()
		{
			setColors();
			SetTexts();
		}

		public void Unload()
		{
		}

		public void LoadControl()
		{
			var loggedOnPerson = ((ITeleoptiPrincipalForLegacy) TeleoptiPrincipal.CurrentPrincipal).UnsafePerson();
			_presenter = new ChangePasswordPresenter(this, _container.Resolve<IChangePassword>(), loggedOnPerson);
			_presenter.Initialize();
			labelSubHeader2.Text = string.Concat(labelSubHeader2.Text, " ", loggedOnPerson.Name);
		}

		public void  SaveChanges()
		{
			_presenter.Save();
		}

		public void SetUnitOfWork(IUnitOfWork value)
		{
		}

		public void Persist()
		{}

		public TreeFamily TreeFamily()
		{
			return new TreeFamily(UserTexts.Resources.MyProfile);
		}

		public string TreeNode()
		{
			return UserTexts.Resources.ChangeYourPassword;
		}

		public void OnShow()
		{
		}

		private void setColors()
		{
			BackColor = ColorHelper.WizardBackgroundColor();
			tableLayoutPanelBody.BackColor = ColorHelper.WizardBackgroundColor();

			gradientPanelHeader.BackColor = ColorHelper.OptionsDialogHeaderBackColor();
			labelHeader.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();

			tableLayoutPanelSubHeader2.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader2.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader2.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
		}

		public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
		{
			throw new NotImplementedException();
		}

		public ViewType ViewType
		{
			get { return ViewType.ChangeYourPassword; }
		}

		public void SetInputFocus()
		{
			textBoxExtPassword.Focus();
		}

		public void SetOldPasswordValid(bool valid)
		{
			autoLabelValidationPassword.Visible = !valid;
		}

		public void SetConfirmPasswordValid(bool valid)
		{
			autoLabelValidationConfirmNewPassword.Visible = !valid;
		}

		public void SetNewPasswordValid(bool valid)
		{
			autoLabelValidationNewPassword.Visible = !valid;
		}

		public void Close()
		{
			_canClose = true;
		}

		public void ShowValidationError()
		{
			_canClose = false;
			ViewBase.ShowErrorMessage(UserTexts.Resources.ChangePasswordValidationError,UserTexts.Resources.ValidationError);
		}

		public bool CanClose()
		{
			return _canClose;
		}

		private void textBoxExtPasswordTextChanged(object sender, EventArgs e)
		{
			_presenter.SetOldPassword(textBoxExtPassword.Text);
		}

		private void textBoxExtNewPasswordTextChanged(object sender, EventArgs e)
		{
			_presenter.SetNewPassword(textBoxExtNewPassword.Text);
		}

		private void textBoxExtConfirmPasswordTextChanged(object sender, EventArgs e)
		{
			_presenter.SetConfirmNewPassword(textBoxExtConfirmPassword.Text);
		}
	}
}
