(function () {

	angular.module('wfm.teamSchedule').filter('names', namesFilter);

	function namesFilter() {
		return function (agents) {
			if (!agents) return '';
			return agents.map(function(agent) { return agent.Name; }).join(', ');
		}
	}

})();