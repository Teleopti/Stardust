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

	that.openInNewTab = function (value) {
		var rooturl = window.location.protocol + "//" + window.location.host + "/Anywhere#";
		var win = window.open(rooturl + value, '_blank');
		win.focus();
	}
	return that;
});
