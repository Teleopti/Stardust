
Teleopti.Start.Authentication.BusinessUnitSelectionViewModel = function (data) {
	var self = this;
	this.BusinessUnits = ko.observableArray();

	this.LoadBusinessUnits = function () {
		data.authenticationState.GetDataForBusinessUnitSelectionView({
			data: {
				type: data.authenticationType,
				datasource: data.dataSourceName
			},
			businessunits: function (businessunits) {
				self.BusinessUnits.removeAll();
				ko.utils.arrayForEach(businessunits, function (bu) {
					$.extend(bu, data);
					var businessUnit = new Teleopti.Start.Authentication.BusinessUnitViewModel(bu);
					self.BusinessUnits.push(businessUnit);
				});
			}
		});
	};
};
