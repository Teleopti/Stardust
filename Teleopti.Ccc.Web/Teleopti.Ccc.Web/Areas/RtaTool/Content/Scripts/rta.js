﻿define([
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

		},
		FetchAgents : function() {
			return [
				{ name: 'Ashley Andeen', usercode: '0085' },
				{ name: 'Dmoitry Pavlov', usercode: '2003' },
				{ name: 'John King', usercode: '2000' },
				{ name: 'John Smith', usercode: '0019' },
				{ name: 'Jon Kleinsmith', usercode: '2001' },
				{ name: 'Juancho Banaag', usercode: '0202' },
				{ name: 'Pierre Baldi', usercode: '0063' },
				{ name: 'Prashant Arora', usercode: '0068' },
				{ name: 'Robert Klashner', usercode: '2002' },
				{ name: 'Stephen Bay', usercode: '0238' }
			];
		}
	}

	
});

