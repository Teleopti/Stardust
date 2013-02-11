
Teleopti.Start.Authentication.MenuViewModel = function(data) {
	var self = this;
	this.Applications = ko.observableArray();

	this.LoadApplications = function() {
		data.authenticationState.GetDataForMenu({
			applications: function(applications) {
				self.Applications([]);
				var map = ko.utils.arrayMap(applications, function(a) {
					$.extend(a, data);
					return new Teleopti.Start.Authentication.ApplicationViewModel(a);
				});
				self.Applications.push.apply(self.Applications, map);
			}
		});
	};
	
};
