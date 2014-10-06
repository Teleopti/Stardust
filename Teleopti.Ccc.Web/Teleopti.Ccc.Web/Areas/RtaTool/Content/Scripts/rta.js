define([
	'jquery'
], function (
	$
) {

	return {
		ServerCall : function(agentState) {

			$.ajax({
				url: 'Rta/Service/SaveExternalUserState',
				type: 'POST',
				data: JSON.stringify(agentState),
				dataType : 'json',
				contentType: "application/json"
			});

		}
	}

	
});

