
Teleopti.Start.Authentication.BusinessUnitSelectionViewModel = function (data) {
	var self = this;
	this.BusinessUnits = ko.observableArray();

	this.LoadBusinessUnits = function() {
		$.ajax({
			url: baseUrl + "Start/AuthenticationApi/BusinessUnits",
			dataType: 'json',
			data: _buildAuthenticationModel(),
			success: function(data, textStatus, jqXHR) {
				for (var i = 0; i < data.length; i++) {
					var businessUnit = new Teleopti.Start.BusinessUnitViewModel();
					businessUnit.Selected(false);
					businessUnit.Name = data[i].Name;
					businessUnit.Id = data[i].Id;
					self.AvailableBusinessUnits.push(businessUnit);
				}
			},
			error: function(jqXHR, textStatus, errorThrown) {
				if (jqXHR.status == 400) {
					var data = $.parseJSON(jqXHR.responseText);
					self.ErrorMessage(data.Errors);
				}
			}
		});
	};
};
