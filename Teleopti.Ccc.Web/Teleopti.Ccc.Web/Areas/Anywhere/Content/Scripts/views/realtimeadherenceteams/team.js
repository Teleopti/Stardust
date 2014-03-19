define([
		'knockout'
],
	function (ko) {
		return function () {

			var that = {};
			that.OutOfAdherence = ko.observable();
			return that;
		};
	}
);