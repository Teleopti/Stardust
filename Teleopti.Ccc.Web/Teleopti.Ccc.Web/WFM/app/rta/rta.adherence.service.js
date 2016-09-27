(function () {
	'use strict';

	angular.module('wfm.rta').service('RtaAdherenceService', [
		function () {
			var service = {};

			service.updateAdherence = function (stuff, adherences) {
				adherences.forEach(function (adherence) {
					var match = stuff.filter(function (item) {
						return item.Id === adherence.Id;
					});
					if (match.length > 0)
						match[0].OutOfAdherence = adherence.OutOfAdherence ? adherence.OutOfAdherence : 0;
				});
			}

			return service;
		}
	]);
})();
