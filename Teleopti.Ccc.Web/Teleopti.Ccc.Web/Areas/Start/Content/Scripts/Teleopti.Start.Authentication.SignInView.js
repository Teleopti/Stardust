
Teleopti.Start.Authentication.SignInView = function (args) {

	var events = new ko.subscribable();

	this.Display = function (data) {
		var viewModel = new Teleopti.Start.Authentication.SignInViewModel({
			baseUrl: args.baseUrl,
			events: events,
			authenticationState: args.authenticationState
		});
		data.render(args.html);
		ko.applyBindings(viewModel, data.element[0]);
		viewModel.LoadDataSources();

		Teleopti.Start.Common.Layout.ActivatePlaceHolderText();
	};



};
