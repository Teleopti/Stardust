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
			//This 'requestsRefactor' string refer to toggle Wfm_Requests_Refactoring_45470 which
			//will be replaced with 'requests' after the refactor version is tested and confirmed to be stable
			if($state.current.name.indexOf('requestsRefactor') > -1)
				vm.helpUrl = 'https://wiki.teleopti.com/TeleoptiWFM/' + 'Requests' + '.' + $state.current.name.split('.')[1];
			else if($state.current.name.indexOf('requestsOrigin') > -1)
				vm.helpUrl = 'https://wiki.teleopti.com/TeleoptiWFM/' + 'Requests';
			else
				vm.helpUrl = 'https://wiki.teleopti.com/TeleoptiWFM/' + $state.current.name;

		});
	}
})();
