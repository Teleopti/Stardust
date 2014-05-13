define([
		'knockout',
		'resources'
],
	function (ko,
		resources) {
	return function () {

		var that = {};

		that.resources = resources;

		that.agents = ko.observableArray(); 
		
		that.update = function (data) {
		};

		return that;
	};
}
);