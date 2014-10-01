
define([
    'jquery'
], function (
    $
	) {
		return {
			get: function(url) {
				return $.get(url);
			}
		}
});

