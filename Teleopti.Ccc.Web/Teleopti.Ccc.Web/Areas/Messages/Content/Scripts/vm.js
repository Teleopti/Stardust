define([
		'knockout',
		'resources'
], function (
	ko,
	resources
	) {

	return function () {
		var self = this;

		self.MyTimeVisible = ko.observable();
		self.AnywhereVisible = ko.observable();
		self.UserName = ko.observable();

		self.ErrorMessage = ko.observable("");

		self.Receivers = ko.observableArray();
		self.Resources = resources;
		
	};
});

