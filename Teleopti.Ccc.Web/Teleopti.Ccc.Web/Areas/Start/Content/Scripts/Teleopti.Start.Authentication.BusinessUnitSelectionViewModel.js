
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
				self.BusinessUnits([]);
				var map = ko.utils.arrayMap(businessunits, function (bu) {
					$.extend(bu, data);
					return new Teleopti.Start.Authentication.BusinessUnitViewModel(bu);
				});
				self.BusinessUnits.push.apply(self.BusinessUnits, map);
			}
		});
	};
};
