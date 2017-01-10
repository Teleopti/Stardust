(function() {
	'use strict';

	angular.module('wfm.rta')
		.factory('rtaAdherenceService', rtaAdherenceService);

	function rtaAdherenceService() {

		var service = {
			updateAdherence: updateAdherence
		};

		return service;
		///////////////////////////

		function updateAdherence(stuff, adherences) {
			adherences.forEach(function(adherence) {
				var match = stuff.filter(function(item) {
					return item.Id === adherence.Id;
				});
				if (match.length > 0)
					match[0].OutOfAdherence = adherence.OutOfAdherence ? adherence.OutOfAdherence : 0;
			});
		}
	};
})();
