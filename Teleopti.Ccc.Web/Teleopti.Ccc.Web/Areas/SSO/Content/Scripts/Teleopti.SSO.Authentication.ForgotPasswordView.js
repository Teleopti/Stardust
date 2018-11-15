Teleopti.SSO.Authentication.ForgotPasswordView = function (data) {
	this.Display = function (viewInfo) {
		var viewModel = new Teleopti.SSO.Authentication.ForgotPasswordViewModel({
			baseUrl: Teleopti.SSO.Authentication.Settings.baseUrl
		});
		viewInfo.render(data.html);
		ko.applyBindings(viewModel, viewInfo.element[0]);

		Teleopti.SSO.Common.Layout.ActivatePlaceHolderText();
	};
};

