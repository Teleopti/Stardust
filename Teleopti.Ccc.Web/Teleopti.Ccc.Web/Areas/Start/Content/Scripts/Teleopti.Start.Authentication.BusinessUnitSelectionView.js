
Teleopti.Start.Authentication.BusinessUnitSelectionView = function (args) {
	this.Display = function (viewInfo) {
		var viewModel = new Teleopti.Start.Authentication.BusinessUnitSelectionViewModel({
			baseUrl: Teleopti.Start.Authentication.Settings.baseUrl,
			authenticationType: viewInfo.authenticationType,
			dataSourceName: viewInfo.dataSourceName,
			authenticationState: args.authenticationState
		});
		viewInfo.render(args.html);
		ko.applyBindings(viewModel, viewInfo.element[0]);
		viewModel.LoadBusinessUnits();
	};
};
