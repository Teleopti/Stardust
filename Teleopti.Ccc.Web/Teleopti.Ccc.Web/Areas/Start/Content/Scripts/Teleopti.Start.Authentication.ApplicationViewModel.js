
Teleopti.Start.Authentication.ApplicationViewModel = function (data) {
	var self = this;
	
	this.Name = data.Name;
	this.Area = data.Area;
	
	this.Url = ko.computed(function () {
		return data.baseUrl + self.Area;
	});
};
