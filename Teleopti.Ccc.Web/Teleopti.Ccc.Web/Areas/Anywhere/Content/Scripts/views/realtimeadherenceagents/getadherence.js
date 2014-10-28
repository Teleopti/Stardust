define([
	'ajax'
], function (
	ajax
) {

	return {
		ServerCall: function (agentState) {
			ajax.ajax({
				url: 'Adherence/ForToday?PersonId='+agentState.PersonId,
				type: 'GET',
				//accepts: 'application/json',
				success: function (data) {
					agentState.HistoricalAdherence(data.AdherencePercent);
				}
			});
		}
	}
});

