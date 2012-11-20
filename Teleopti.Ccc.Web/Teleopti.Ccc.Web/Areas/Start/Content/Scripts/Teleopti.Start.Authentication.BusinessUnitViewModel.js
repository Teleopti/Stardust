
Teleopti.Start.Authentication.BusinessUnitViewModel = function (data) {
	var self = this;

	this.Id = data.Id;
	this.Name = data.Name;

	this.Select = function () {
		data.authenticationState.SignIn({
			data: {
				businessUnitId: self.Id
			},
			success: function() {
				Teleopti.Start.Authentication.Navigation.GotoMenu();
			},
			error: function () {
				Teleopti.Start.Authentication.Navigation.GotoSignIn();
			}
		});

	};

};
