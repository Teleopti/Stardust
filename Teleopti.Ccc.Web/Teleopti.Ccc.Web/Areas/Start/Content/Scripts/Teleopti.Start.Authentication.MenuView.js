
Teleopti.Start.Authentication.MenuView = function (data) {
	this.Display = function (viewInfo) {
		viewInfo.render(data.html);
		var viewModel = new Teleopti.Start.Authentication.MenuViewModel({
			baseUrl: Teleopti.Start.Authentication.Settings.baseUrl
		});
		ko.applyBindings(viewModel, viewInfo.element[0]);
		viewModel.LoadApplications();
	};
};
