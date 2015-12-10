define([
	'ajax'
], function (
	ajax
) {

	return {
		ServerCall: function (callback, personId) {
			ajax.ajax({
				url: 'Adherence/ForToday?PersonId=' + personId,
				type: 'GET',
				accepts: 'application/json',
				success: callback
			});
		}
	}
});

