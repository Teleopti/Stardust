(function () {
	var rta = angular.module('wfm.rta', [
		'ui.grid',
		'ui.grid.autoResize',
		'ui.grid.resizeColumns',
		'ui.grid.selection',
		'ngResource', 'ui.router', 'ngStorage', 'toggleService', 'wfm.notice', 'pascalprecht.translate']);

		rta.run([
		'$rootScope', '$state', '$location', function ($rootScope, $state, $location) {
			$rootScope.$on('$stateChangeSuccess',
				function (event, toState) {
					if ($location.url() == $state.current.url && toState.name == 'rta') $state.go('rta.sites');
				});
		}
]);
})();
