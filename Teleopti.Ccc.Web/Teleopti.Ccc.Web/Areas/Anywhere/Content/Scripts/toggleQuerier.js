define([
	'ajax'
], function (ajax) {
	return function (toggleName, callbacks) {
		ajax.ajax({
			url: 'ToggleHandler/IsEnabled?toggle=' + toggleName,
			success: function (data) {
				var callback;
				if (data.IsEnabled) {
					callback = callbacks.enabled;
				} else {
					callback = callbacks.disabled;
				}
				if (typeof (callback) === typeof (Function)) {
					callback();
				}
			}
		});
	}
});