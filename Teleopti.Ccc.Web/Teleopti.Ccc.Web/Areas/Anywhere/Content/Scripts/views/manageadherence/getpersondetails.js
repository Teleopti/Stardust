define([
	'ajax'
], function (
	ajax
) {
	return {
		ServerCall: function (callback, personId) {
			ajax.ajax({
				url: "Agents/PersonDetails?PersonId=" + personId,
				success: callback
			});
		}
	}
});

