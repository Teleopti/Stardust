if (typeof(Teleopti) === 'undefined') {
	Teleopti = {};
}
if (typeof (Teleopti.Start) === 'undefined') {
	Teleopti.Start = {};
}
if (typeof (Teleopti.Start.Authentication) === 'undefined') {
	Teleopti.Start.Authentication = {};
}

Teleopti.Start.Authentication.ApplicationViewModel = function (data) {
	var self = this;
	
	this.Name = data.Name;
	this.Area = data.Area;
	
	this.Url = ko.computed(function () {
		return data.baseUrl + self.Area;
	});
};
