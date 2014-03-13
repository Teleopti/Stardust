define([
		'knockout',
		'views/realtimeadherence/site'
],
	function (ko, site) {
	return function () {

		var that = {};
		that.sites = ko.observableArray();
		that.fill = function(data) {
			that.sites(data);
		};

		that.update = function(data) {
			var updatedSite = site();
			updatedSite.OutOfAdherence(data.OutOfAdherence);
			that.sites.push(updatedSite);
		};

		return that;
	};
}
);