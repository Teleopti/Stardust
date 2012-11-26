
Teleopti.Start.Authentication.DataSourceViewModel = function (data) {
	var self = this;
	this.Name = "";
	this.Type = "";
	this.Selected = ko.observable(false);
	this.Select = function () {
		data.events.notifySubscribers(self, 'dataSourceSelected');
	};
};
