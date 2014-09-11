define([
], function(
) {
	var ianaTimeZone;
	return {
		IanaTimeZone: function () { return ianaTimeZone; },
		SetIanaTimeZone: function (timeZone) {
			ianaTimeZone = timeZone;
		}
	};
});
