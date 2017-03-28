(function () {
angular
.module('wfm.rta', [
		'ui.grid',
		'ui.grid.autoResize',
		'ui.grid.resizeColumns',
		'ui.grid.selection',
		'ngResource',
		'ui.router',
		'ngStorage',
		'toggleService',
		'wfm.notice',
		'pascalprecht.translate',
		'currentUserInfoService'
	])
.run(runRtaModule);

runRtaModule.$inject = ['$rootScope', '$state', '$location'];

function runRtaModule($rootScope, $state, $location) {
	var result = $rootScope.$on('$stateChangeSuccess',
		function (event, toState) {
			if ($location.url() == $state.current.url && toState.name == 'rta') $state.go('rta.sites');
		});
	return result;
}
})();
