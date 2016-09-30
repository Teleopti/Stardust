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

//
//
//
// (function() {
// 	'use strict';
// 	var help = angular.module('wfm.help', []);
//
// 	help.controller('HelpController', [
// 		'$scope', 'HelpService', 'Toggle', function ($scope, HelpService, toggleService) {
// 			$scope.help = HelpService;
// 			$scope.displayHelp = false;
// 			if(toggleService.Wfm_DisplayOnlineHelp_39402){
// 				$scope.displayHelp = true;
// 			}
// 		}
// 	]);
// })();
