define([
		'knockout'
],
	function (ko) {
	return function () {

		var that = {};
		that.sites = ko.observableArray();
		that.fill = function(data) {
			that.sites(data);
		};
		
		return that;
	};
}
);