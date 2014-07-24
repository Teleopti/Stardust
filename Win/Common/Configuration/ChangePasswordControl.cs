using System;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.WinCode.Common.Configuration;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Common.Configuration
{
	/// <summary>
	/// Change your password control
	/// </summary>
	public partial class ChangePasswordControl : BaseUserControl, ISettingPage, IChangePasswordView, ICheckBeforeClosing
	{
		private ChangePasswordPresenter _presenter;
		private bool _canClose;

		/// <summary>
		/// Manually initialze control components. Calls when OptionDialog contructor.
		/// </summary>
		/// <remarks>
		/// Created by: Aruna Priyankara Wickrama
		/// Created date: 2008-04-07
		/// </remarks>
		public void InitializeDialogControl()
		{
			setColors();
			SetTexts();
		}

		public void Unload()
		{
		}

		/// <summary>
		/// Binds the control with data from repository
		/// </summary>
		public void LoadControl()
		{
			_presenter = new ChangePasswordPresenter(this,
													 new PasswordPolicy(
														 StateHolderReader.Instance.StateReader.ApplicationScopeData.
															 LoadPasswordPolicyService),
													 UnitOfWorkFactory.Current,
													 new RepositoryFactory(), new OneWayEncryption());
			_presenter.Initialize();
			labelSubHeader2.Text = string.Concat(labelSubHeader2.Text, " ",
												 ((IUnsafePerson)TeleoptiPrincipal.Current).Person.Name);
		}

		/// <summary>
		/// Saves the changes to the repository
		/// </summary>
		public void  SaveChanges()
		{
			_presenter.Save();
		}

		/// <summary>
		/// Initializes the contract control.
		/// </summary>
		public ChangePasswordControl()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Sets the unit of work
		/// </summary>
		/// <param name="value"></param>
		public void SetUnitOfWork(IUnitOfWork value)
		{
		}

		/// <summary>
		/// persist all to the persistant storage
		/// </summary>
		public void Persist()
		{}

		/// <summary>
		/// The tree family
		/// </summary>
		/// <returns></returns>
		public TreeFamily TreeFamily()
		{
			return new TreeFamily(UserTexts.Resources.MyProfile);
		}

		/// <summary>
		/// The tree node
		/// </summary>
		/// <returns></returns>
		public string TreeNode()
		{
			return UserTexts.Resources.ChangeYourPassword;
		}

		public void OnShow()
		{
		}

		/// <summary>
		/// Sets colors for the control.
		/// </summary>
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

		private void TextBoxExtPasswordTextChanged(object sender, EventArgs e)
		{
			_presenter.SetOldPassword(textBoxExtPassword.Text);
		}

		private void TextBoxExtNewPasswordTextChanged(object sender, EventArgs e)
		{
			_presenter.SetNewPassword(textBoxExtNewPassword.Text);
		}

		private void TextBoxExtConfirmPasswordTextChanged(object sender, EventArgs e)
		{
			_presenter.SetConfirmNewPassword(textBoxExtConfirmPassword.Text);
		}
	}
}
