
Teleopti.SSO.Authentication.ChangePasswordViewModel = function (data) {
	var self = this;
	this.MustChangePassword = ko.observable(data.mustChangePassword);
	this.NewPassword = ko.observable('');
	this.ConfirmNewPassword = ko.observable('');
	this.OldPassword = ko.observable('');

	this.ErrorMessage = ko.observable('');

	this.IsMatched = ko.computed(function () {
		if (!self.NewPassword().length) return false;
		return self.NewPassword() == self.ConfirmNewPassword();
	});

	this.ApplyChangePassword = function () {
		if (!self.IsMatched()) {
			self.ErrorMessage($('#Password-change-error').data('notmatch'));
			return;
		}
		self.ErrorMessage('');
		data.authenticationState.ApplyChangePassword({
			baseUrl: data.baseUrl,
			data: {
				dataSourceName: data.dataSourceName,
				newPassword: self.NewPassword(),
				oldPassword: self.OldPassword()
			},
			errormessage: function (message) {
				self.ErrorMessage(message);
			}
		});
	};

	this.SkipChangePassword = function () {
		data.authenticationState.GotoReturnUrl();
	};
};
