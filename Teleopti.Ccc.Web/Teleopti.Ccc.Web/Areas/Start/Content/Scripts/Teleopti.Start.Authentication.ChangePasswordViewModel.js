
Teleopti.Start.Authentication.ChangePasswordViewModel = function (data) {
	var self = this;
	this.MustChangePassword = ko.observable(data.mustChangePassword);

	this.SkipChangePassword = function () {
		data.authenticationState.AttemptGotoApplicationBySignIn({
			baseUrl: data.baseUrl
		});
	};
};
