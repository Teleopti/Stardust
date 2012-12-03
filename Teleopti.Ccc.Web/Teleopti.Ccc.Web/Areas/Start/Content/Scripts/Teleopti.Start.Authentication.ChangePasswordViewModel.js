﻿
Teleopti.Start.Authentication.ChangePasswordViewModel = function (data) {
	var self = this;
	this.MustChangePassword = ko.observable(data.mustChangePassword);
	this.NewPassword = ko.observable('');
	this.ConfirmNewPassword = ko.observable('');
	this.OldPassword = ko.observable('');

	this.ErrorMessage = ko.observable('');

	this.ApplyChangePassword = function () {
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
		data.authenticationState.AttemptGotoApplicationBySignIn({
			baseUrl: data.baseUrl
		});
	};
};
