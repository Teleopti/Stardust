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

	that.baseLocation = function () {
		if (!window.location.origin) {
			window.location.origin = window.location.protocol + "//" + window.location.hostname + (window.location.port ? ':' + window.location.port : '');
		}
		return window.location.origin + window.location.pathname;
	};
	return that;
});
