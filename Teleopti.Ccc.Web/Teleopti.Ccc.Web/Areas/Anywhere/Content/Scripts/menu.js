define([
		'knockout'
	], function(ko) {

		return function(resources) {
			var self = this;

			self.Resources = resources;
			self.MyTimeVisible = ko.observable(false);
			self.MobileReportsVisible = ko.observable(false);
			self.RealTimeAdherenceVisible = ko.observable(false);
			self.ActiveView = ko.observable("");
			self.UserName = ko.observable("");
		};
	});
