
Teleopti.Start.Authentication.BusinessUnitSelectionViewModel = function (data) {
	var self = this;
	this.BusinessUnits = ko.observableArray();
	this.ErrorMesg = ko.observable();
	this.HasErrors = ko.computed(function () {
		var errorMessage = self.ErrorMesg();
		if (typeof (errorMessage) == 'undefined')
			return false;
		return errorMessage && errorMessage.length > 0;
	});

	this.LoadBusinessUnits = function () {
		data.authenticationState.GetDataForBusinessUnitSelectionView({
			data: {
			},
			businessunits: function (businessunits) {
				self.BusinessUnits([]);
				var map = ko.utils.arrayMap(businessunits, function (bu) {
					$.extend(bu, data);
					$.extend(bu, {
						businessUnitError: function (mesg) {
							self.ErrorMesg(mesg);
						}
					});
					return new Teleopti.Start.Authentication.BusinessUnitViewModel(bu);
				});
				self.BusinessUnits.push.apply(self.BusinessUnits, map);
			}
		});
	};
};
