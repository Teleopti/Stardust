define([
		'knockout'
	], function(ko) {

		return function(translations) {
			var self = this;

			this.Translations = translations;
			this.MyTimeVisible = ko.observable(false);
			this.MobileReportsVisible = ko.observable(false);
			this.ActiveView = ko.observable("");
			this.UserName = ko.observable("");

		};
	});
