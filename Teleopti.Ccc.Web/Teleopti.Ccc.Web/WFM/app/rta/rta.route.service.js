(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.factory('rtaRouteService', rtaRouteService);

	rtaRouteService.$inject = ['$state', 'Toggle'];

	function rtaRouteService($state, Toggle) {

		return {
			goToOverview: function () { $state.go('refact-rta'); },
			goToAgents: function (ids) { $state.go('rta.agents', ids); },
			goToSelectSkill: function () { $state.go('rta.agents'); },
			urlForChangingSchedule: function (personId) { return $state.href('teams.for', { personId: personId }) },
			urlForHistoricalAdherence: function (personId) { return $state.href('rta.historical', { personId: personId }); }
		}

	};
})();
