define([
		'knockout'
	], function(ko) {

		return function(resources) {
			var self = this;

			this.Resources = resources;
			this.MyTimeVisible = ko.observable(false);
			this.MobileReportsVisible = ko.observable(false);
			this.RealTimeAdherenceVisible = ko.observable(false);
			this.ActiveView = ko.observable("");
			this.UserName = ko.observable("");
		};
	});
