﻿
Teleopti.Start.Authentication.SignInView = function (args) {
	this.Display = function (data) {
		var viewModel = new Teleopti.Start.Authentication.SignInViewModel({
			baseUrl: args.baseUrl,
			authenticationState: args.authenticationState
		});
		data.render(args.html);
		ko.applyBindings(viewModel, data.element[0]);
		viewModel.LoadDataSources();
		viewModel.UserNameFocus.subscribe(function (newValue) {
			if (newValue == true) {
				$('#Username-input').focus();
			}
		});

		Teleopti.Start.Common.Layout.ActivatePlaceHolderText();
	};
};
