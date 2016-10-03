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
		if(Toggle.Wfm_DisplayOnlineHelp_39402){
			vm.displayHelp = true;
		}

		$rootScope.$on('$stateChangeSuccess', function(event, next, toParams) {
			vm.helpUrl = 'https://wiki.teleopti.com/TeleoptiWFM/' + $state.current.name;
		});
	}
})();
