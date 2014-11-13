define([
	'jquery'
], function (
	$
) {

	return {
		ServerCall : function(agentState) {

			$.ajax({
				url: 'Rta/State/Change',
				type: 'POST',
				data: JSON.stringify(agentState),
				dataType : 'json',
				contentType: "application/json"
			});

		}
	}

	
});

