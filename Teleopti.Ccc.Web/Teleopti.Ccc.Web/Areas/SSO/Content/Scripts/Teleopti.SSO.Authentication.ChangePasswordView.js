
Teleopti.SSO.Authentication.ChangePasswordView = function (data) {
	this.Display = function (viewInfo) {

		if (!data.authenticationState.CheckState()) {
			Teleopti.SSO.Authentication.Navigation.GotoSignIn();
			return;
		}
		var viewModel = new Teleopti.SSO.Authentication.ChangePasswordViewModel({
			baseUrl: Teleopti.SSO.Authentication.Settings.baseUrl,
			mustChangePassword: viewInfo.mustChangePassword,
			authenticationState: data.authenticationState
		});
		viewInfo.render(data.html);
		ko.applyBindings(viewModel, viewInfo.element[0]);

		Teleopti.SSO.Common.Layout.ActivatePlaceHolderText();
	};
};