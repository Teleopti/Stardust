define([], function() {
	return function () {

		var obj = {};
		
		obj.fill = function(data) {
			obj.sites = data;
		};

		return obj;
	};
}
);