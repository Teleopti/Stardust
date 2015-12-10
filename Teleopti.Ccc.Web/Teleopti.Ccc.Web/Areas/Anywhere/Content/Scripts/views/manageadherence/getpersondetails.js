define([
	'ajax'
], function (
	ajax
) {
	return {
		ServerCall: function (callback, personId) {
			ajax.ajax({
				url: "api/Agents/PersonDetails?PersonId=" + personId,
				success: callback
			});
		}
	}
});

