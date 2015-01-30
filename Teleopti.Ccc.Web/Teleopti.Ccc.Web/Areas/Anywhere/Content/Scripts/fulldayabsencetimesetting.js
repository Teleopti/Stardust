define([
	'ajax'
], function (ajax) {

	var dataFetch = ajax.ajax({
		url: "Anywhere/Application/FullDayAbsenceRequestTimeSetting"
	});

	return {
		get: function () {
			return dataFetch;
		}
	};
});