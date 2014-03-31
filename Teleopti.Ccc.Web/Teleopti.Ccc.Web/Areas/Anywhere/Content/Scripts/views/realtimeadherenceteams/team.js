define([
		'knockout'
],
	function (ko) {
		return function () {

			var that = {};
			that.OutOfAdherence = ko.observable();
			that.hasBeenUpdated = ko.observable(false);
			return that;
		};
	}
);