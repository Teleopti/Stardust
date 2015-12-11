define([
	'ajax'
], function (
	ajax
) {

	return {
		ServerCall: function (callback, personId) {
			ajax.ajax({
				url: 'api/Adherence/ForDetails?PersonId=' + personId,
				type: 'GET',
				accepts: 'application/json',
				success: callback
			});
		}
	}
});

