﻿
Teleopti.SSO.Authentication.SignInViewModel = function (data) {
	var self = this;

	this.UserName = ko.observable('');
	this.Password = ko.observable('');
	this.IsLogonFromBrowser = typeof isTeleoptiProvider === 'undefined';
	this.RememberMe = ko.observable(!self.IsLogonFromBrowser);
	this.ErrorMessage = ko.observable();
	this.HasErrorMessage = ko.computed(function () {
		var errorMessage = self.ErrorMessage();
		return errorMessage && errorMessage.length > 0;
	});
	this.Ajax = new Teleopti.SSO.Authentication.JQueryAjaxViewModel();

	this.SignIn = function () {
		var state = data.authenticationState;
		self.ErrorMessage('');
		state.TryToSignIn({
			data: {
				type: "application",
				username: self.UserName(),
				password: self.Password(),
				rememberMe: self.RememberMe(),
				IsLogonFromBrowser: self.IsLogonFromBrowser
			},
			errormessage: function (message) {
				self.ErrorMessage(message);
			}
		});
	};
};
