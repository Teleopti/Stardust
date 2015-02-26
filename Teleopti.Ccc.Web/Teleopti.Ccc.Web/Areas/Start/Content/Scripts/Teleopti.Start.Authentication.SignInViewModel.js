﻿
Teleopti.Start.Authentication.SignInViewModel = function (data) {
	var self = this;

	this.ErrorMessage = ko.observable();
	this.ShowSignInAsAnotherUser = ko.observable(false);
	this.HasErrorMessage = ko.computed(function () {
		var errorMessage = self.ErrorMessage();
		return errorMessage && errorMessage.length > 0;
	});

	this.Ajax = new Teleopti.Start.Authentication.JQueryAjaxViewModel();

	this.SignIn = function () {
		var state = data.authenticationState;

		self.ErrorMessage('');

		state.TryToSignIn({
			data: {
			},
			errormessage: function (message) {
				self.ErrorMessage(message);
			},
			nobusinessunit: function () {
				self.ErrorMessage($('#Signin-error').data('nobusinessunitext'));
			}
		});

	};

};
