
define([
    'jquery'
], function (
    $
	) {
		return {
			get: function(url, data) {
				return $.get(url, data);
			}
		}
});

