Teleopti.SSO.Authentication.ForgotPasswordViewModel = function (data) {
	var self = this;
	this.UserName = ko.observable('');
	this.ErrorMessage = ko.observable('');
	this.SuccessMessage = ko.observable('');
	this.baseUrl = data.baseUrl;
	
	this.SendForgotPasswordMail = function () {
		$.ajax({
			type: "POST",
			data: { UserIdentifier: self.UserName },
			url: self.baseUrl + "ChangePassword/RequestReset",
			success: function (data) {
				if (data.success === true) {
					self.ShowSuccessMessage();
				} else {
					self.ShowErrorMessage($('#Password-change-error').data('techerror'));
				}
			},
			error: function (error) {
				self.ShowErrorMessage($('#Password-change-error').data('techerror'));
			}
		});

	};

	this.ShowErrorMessage = function (errorMessage) {
		self.ErrorMessage(errorMessage);
		self.SuccessMessage('');
	};

	this.ShowSuccessMessage = function () {
		self.ErrorMessage('');
		self.SuccessMessage('show');
	};

	this.BackToLogin = function () {
		Teleopti.SSO.Authentication.Navigation.GotoSignIn();
	};

	this.onEnterKey = function (d, e) {
		if (e.keyCode === 13) {
			self.SendForgotPasswordMail();
		}
		return true;
	};
};