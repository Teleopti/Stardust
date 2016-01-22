
Teleopti.SSO.Authentication.SignInViewModel = function (data) {
	var self = this;

	this.UserName = ko.observable('');
	this.Password = ko.observable('');
	this.IsLogonFromFatClient = typeof isTeleoptiProvider !== 'undefined';
	this.RememberMe = ko.observable(self.IsLogonFromFatClient);
	this.ErrorMessage = ko.observable();
	this.HasErrorMessage = ko.computed(function () {
		var errorMessage = self.ErrorMessage();
		return errorMessage && errorMessage.length > 0;
	});

	this.Ajax = new Teleopti.SSO.Authentication.JQueryAjaxViewModel();

	this.SignIn = function () {
		if (!self.UserName() || !self.Password())
			return;

		var state = data.authenticationState;
		self.ErrorMessage('');
		state.TryToSignIn({
			data: {
				type: "application",
				username: self.UserName(),
				password: self.Password(),
				rememberMe: self.RememberMe()
			},
			errormessage: function (message) {
				self.ErrorMessage(message);
			}
		});
	};
};
