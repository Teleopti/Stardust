
Teleopti.Start.Authentication.BusinessUnitViewModel = function (data) {
	var self = this;

	this.Id = data.Id;
	this.Name = data.Name;

	this.Select = function () {
		data.authenticationState.AttemptGotoApplicationBySelectingBusinessUnit({
			baseUrl: data.baseUrl,
			data: {
				businessUnitId: self.Id
			}
		});

	};

};
