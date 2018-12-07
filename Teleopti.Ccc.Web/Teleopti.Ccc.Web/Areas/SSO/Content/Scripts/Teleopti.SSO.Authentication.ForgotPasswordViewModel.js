Teleopti.SSO.Authentication.ForgotPasswordViewModel = function (data) {
	var self = this;
	this.MailAddress = ko.observable('');
	this.ErrorMessage = ko.observable('');
	this.SuccessMessage = ko.observable('');
	this.baseUrl = data.baseUrl;
	var emailRegex = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;

	this.SendForgotPasswordMail = function () {
		if (emailRegex.test(self.MailAddress())) {
			$.ajax({
				type: "POST",
				data: { Email: self.MailAddress },
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
		} else {
			self.ErrorMessage($('#Password-change-error').data('invalid'));
		}
	};

	this.ShowErrorMessage = function(errorMessage) {
		self.ErrorMessage(errorMessage);
		self.SuccessMessage('');
	}

	this.ShowSuccessMessage = function() {
		self.ErrorMessage('');
		self.SuccessMessage('show');
	}

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