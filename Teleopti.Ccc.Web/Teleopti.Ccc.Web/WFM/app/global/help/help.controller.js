(function() {
	'use strict';

	angular
	.module('wfm.help', [])
	.controller('HelpController', HelpController);

	HelpController.$inject = ['$scope', '$rootScope', '$state', 'Toggle'];

	function HelpController($scope, $rootScope, $state, Toggle) {
		var vm = this;

		vm.helpUrl;
		vm.displayHelp = false;

		Toggle.togglesLoaded.then(function () {
			vm.displayHelp = Toggle.Wfm_DisplayOnlineHelp_39402;
		});

		$rootScope.$on('$stateChangeSuccess', function(event, next, toParams) {
			vm.helpUrl = 'https://wiki.teleopti.com/TeleoptiWFM/' + $state.current.name;
		});
	}
})();
