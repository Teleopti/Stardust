define([
	'ajax'
], function (ajax) {
	return function (toggleName, enabledCallback, disabledCallback) {
		ajax.ajax({
			url: 'ToggleHandler/IsEnabled?toggle=' + toggleName,
			success: function (data) {
				if (data.IsEnabled) {
					enabledCallback();
				} else {
					disabledCallback();
				}
			}
		});
	}
});