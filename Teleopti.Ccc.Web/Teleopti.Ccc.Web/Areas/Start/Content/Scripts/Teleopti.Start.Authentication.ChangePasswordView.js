
Teleopti.Start.Authentication.ChangePasswordView = function (data) {
	this.Display = function (viewInfo) {

		var viewModel = new Teleopti.Start.Authentication.ChangePasswordViewModel({
			baseUrl: Teleopti.Start.Authentication.Settings.baseUrl,
			mustChangePassword: viewInfo.mustChangePassword,
			authenticationState: data.authenticationState,
//			authenticationType: viewInfo.authenticationType,
			dataSourceName: viewInfo.dataSourceName
		});
		viewInfo.render(data.html);
		ko.applyBindings(viewModel, viewInfo.element[0]);
	};
};

