define([
		'knockout',
		'resources'
],
	function (
		ko,
		resources
	) {
		return function () {

			var that = {};

			that.resources = resources;

			that.description = ko.observable('xxDiagnosis');

			return that;
		};
	}
);