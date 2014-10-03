define([
	'jquery'
], function (
	$
) {

	return {
		ServerCall : function(agentState) {

			// set this mofo value here god damn it!
			agentState.AuthenticationKey = '!#¤atAbgT%';

			$.ajax({
				url: 'rta/service/SaveExternalUserState',
				type: 'POST',
				data: JSON.stringify(agentState),
				dataType : 'json',
				contentType: "application/json"

			});


		}
	}

	
});

