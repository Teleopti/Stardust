
Teleopti.Start.Authentication.BusinessUnitSelectionView = function (html) {
	this.Display = function (data) {
		data.render(html);
		var viewModel = new Teleopti.Start.Authentication.BusinessUnitSelectionView({
			baseUrl: Teleopti.Start.Authentication.Settings.baseUrl
		});
		data.render(html);
		ko.applyBindings(viewModel, data.element[0]);
		viewModel.LoadBusinessUnits();
	};
};
