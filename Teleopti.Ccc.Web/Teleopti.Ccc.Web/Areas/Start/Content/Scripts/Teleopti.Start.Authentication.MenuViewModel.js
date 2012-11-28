
Teleopti.Start.Authentication.MenuViewModel = function(data) {
	var self = this;
	this.Applications = ko.observableArray();

	this.LoadApplications = function() {
		data.authenticationState.GetDataForMenu({
			applications: function(applications) {
				self.Applications.removeAll();
				ko.utils.arrayForEach(applications, function(a) {
					$.extend(a, data);
					var application = new Teleopti.Start.Authentication.ApplicationViewModel(a);
					self.Applications.push(application);
				});
			}
		});
	};
	
};
