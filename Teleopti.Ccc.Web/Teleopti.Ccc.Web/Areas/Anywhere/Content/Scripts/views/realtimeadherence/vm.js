define([], function() {
	return function () {

		var that = {};
		that.sites = [];
		that.fill = function(data) {
			that.sites = data;
		};
		
		return that;
	};
}
);