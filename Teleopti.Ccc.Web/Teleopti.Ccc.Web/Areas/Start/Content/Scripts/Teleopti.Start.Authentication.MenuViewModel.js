
Teleopti.Start.Authentication.MenuViewModel = function (data) {
	var self = this;
	this.Applications = ko.observableArray();

	this.LoadApplications = function () {
		$.ajax({
			url: data.baseUrl + "Start/Menu/Applications",
			type: 'GET',
			success: function (responseData, textStatus, jqXHR) {
				self.Applications.removeAll();
				for (var i = 0; i < responseData.length; i++) {
					$.extend(responseData[i], data);
					var vm = new Teleopti.Start.Authentication.ApplicationViewModel(responseData[i]);
					self.Applications.push(vm);
				}
			},
			error: function (jqXHR, textStatus, errorThrown) {
				Teleopti.Start.Authentication.Navigation.GotoSignIn();
			}
		});
	};
};
