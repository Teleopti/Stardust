define([
	'ajax'
], function (ajax) {

	var dataFetch = ajax.ajax({
		url: "Anywhere/Application/Permissions"
	});

	return {
		get: function () {
			return dataFetch;
		}
	};
});