
Teleopti.Start.Authentication.BusinessUnitSelectionViewModel = function (data) {
	var self = this;
	this.BusinessUnits = ko.observableArray();

	this.LoadBusinessUnits = function () {
		var state = data.authenticationState;
		state.MightLoadBusinessUnits({
			data: {
				type: data.authenticationType,
				datasource: data.dataSourceName
			},
			success: function(responseData, textStatus, jqXHR) {
				for (var i = 0; i < responseData.length; i++) {
					var businessUnit = new Teleopti.Start.Authentication.BusinessUnitViewModel({
						Id: responseData[i].Id,
						Name: responseData[i].Name,
						authenticationState: state
					});
					self.BusinessUnits.push(businessUnit);
				}
			},
			error: function(jqXHR, textStatus, errorThrown) {
				Teleopti.Start.Authentication.Navigation.GotoSignIn();
			}
		});
	};
};
