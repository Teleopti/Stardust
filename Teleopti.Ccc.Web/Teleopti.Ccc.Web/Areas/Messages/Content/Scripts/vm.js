define([
		'knockout',
		'resources'
], function (
	ko,
	resources
	) {

	return function () {
		var self = this;
		self.ErrorMessage = ko.observable("");
		self.Receivers = ko.observableArray();
		self.Resources = resources;
	};
});

