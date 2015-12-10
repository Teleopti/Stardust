define([
	'ajax'
], function (
	ajax
) {

	return {
		ServerCall: function (callback, personId) {
			ajax.ajax({
				url: 'Adherence/ForDetails?PersonId=' + personId,
				type: 'GET',
				accepts: 'application/json',
				success: callback
			});
		}
	}
});

