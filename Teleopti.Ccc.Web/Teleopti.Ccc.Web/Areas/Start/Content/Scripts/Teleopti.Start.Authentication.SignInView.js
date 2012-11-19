
Teleopti.Start.Authentication.SignInView = function (html) {
	
	var events = new ko.subscribable();
	
	this.Display = function (data) {
		var viewModel = new Teleopti.Start.Authentication.SignInViewModel({
			baseUrl: Teleopti.Start.Authentication.Settings.baseUrl,
			events: events
		});
		data.render(html);
		ko.applyBindings(viewModel, data.element[0]);
		viewModel.LoadDataSources();
	};

};
