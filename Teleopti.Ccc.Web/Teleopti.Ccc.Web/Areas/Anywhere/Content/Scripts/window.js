define([
], function (
	) {

	var that = {};

	that.setLocationHash = function (value) {
		window.location.hash = value;
	};

	that.locationReplace = function (value) {
		window.location.replace(value);
	};

	return that;
});
