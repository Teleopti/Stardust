
Teleopti.SSO.Authentication.SignInView = function (args) {
	this.Display = function (data) {
		var viewModel = new Teleopti.SSO.Authentication.SignInViewModel({
			baseUrl: args.baseUrl,
			authenticationState: args.authenticationState
		});
		data.render(args.html);
		ko.applyBindings(viewModel, data.element[0]);

		Teleopti.SSO.Common.Layout.ActivatePlaceHolderText();

		viewModel.UserNameFocus.subscribe(function (newValue) {
			if (newValue == true) {
				$('#Username-input').focus();
			}
		});
	};
};
