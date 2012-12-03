
Teleopti.Start.Authentication.ChangePasswordViewModel = function (data) {
	var self = this;
	this.MustChangePassword = ko.observable(data.mustChangePassword);
	this.NewPassword = ko.observable('');
	this.ConfirmNewPassword = ko.observable('');
	this.OldPassword = ko.observable('');

	this.ApplyChangePassword = function () {
		data.authenticationState.ApplyChangePassword({
			baseUrl: data.baseUrl,
			data: {
				dataSourceName: data.dataSourceName,
				NewPassword: self.NewPassword(),
				OldPassword: self.OldPassword()
			}
		});
	};

	this.SkipChangePassword = function () {
		data.authenticationState.AttemptGotoApplicationBySignIn({
			baseUrl: data.baseUrl
		});
	};
};
