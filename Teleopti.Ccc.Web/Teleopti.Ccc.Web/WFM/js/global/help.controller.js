(function() {
	'use strict';
	var help = angular.module('wfm.help', []);

	help.controller('HelpController', [
		'$scope', 'HelpService', 'Toggle', function ($scope, HelpService, toggleService) {
			$scope.help = HelpService;
			$scope.displayHelp = false;
			if(toggleService.Wfm_DisplayOnlineHelp_39402){
				$scope.displayHelp = true;
			}
		}
	]);

	help.service('HelpService', ['$translate', function ($translate) {
			var helpService = {};
			helpService.currentHelp = {};

			helpService.updateState = function(state) {
				helpService.currentHelp = {
					name: $translate.instant(state.current.name),
					helpDesc: '',
					helpUrl: 'http://wiki.teleopti.com/TeleoptiWFM/' + state.current.name
				}
			};

			return helpService;
		}
	]);
})();
