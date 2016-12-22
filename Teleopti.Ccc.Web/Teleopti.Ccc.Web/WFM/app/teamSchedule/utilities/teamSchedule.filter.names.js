(function () {
	'use strict';

	angular.module('wfm.teamSchedule').filter('names', namesFilter);

	function namesFilter() {
		return function (agents) {
			if (!agents || agents.length == 0) return '';
			return agents.map(function(agent) { return agent.Name; }).join(', ');
		}
	}

})();