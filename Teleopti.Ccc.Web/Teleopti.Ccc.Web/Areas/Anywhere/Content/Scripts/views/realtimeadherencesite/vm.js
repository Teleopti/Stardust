define([
		'knockout'
],
	function (ko) {
	return function () {

		var that = {};
		that.teams = ko.observableArray();
		that.fill = function(data) {
			that.teams(data);
		};
		
		return that;
	};
}
);