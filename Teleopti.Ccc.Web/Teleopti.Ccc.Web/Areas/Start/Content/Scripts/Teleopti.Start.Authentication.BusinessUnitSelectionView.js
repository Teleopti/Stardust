
Teleopti.Start.Authentication.BusinessUnitSelectionView = function (data) {
	this.Display = function (viewInfo) {
		var viewModel = new Teleopti.Start.Authentication.BusinessUnitSelectionViewModel({
			baseUrl: Teleopti.Start.Authentication.Settings.baseUrl,
			authenticationState: data.authenticationState,
		});
		viewInfo.render(data.html);
		ko.applyBindings(viewModel, viewInfo.element[0]);
		viewModel.LoadBusinessUnits();
	};
};
