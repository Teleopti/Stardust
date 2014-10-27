define([
	'jquery'
], function (
	$
) {

	return {
		ServerCall: function (agentState) {
			$.ajax({
				url: 'Adherence/ForToday?PersonId='+agentState.PersonId,
				type: 'GET',
				//data: JSON.stringify(personId),
				//dataType: 'json',
				contentType: "application/json",
				success: function (data) {
					agentState.HistoricalAdherence(data.AdherencePercent);
				}

			});
		}
	}
});

