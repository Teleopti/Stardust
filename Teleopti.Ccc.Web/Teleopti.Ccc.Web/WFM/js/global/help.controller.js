(function() {
	'use strict';
	var help = angular.module('wfm.help', []);

	help.controller('HelpController', [
		'$scope', 'HelpService', function ($scope, HelpService) {
			$scope.help = HelpService;
		}
	]);

	help.service('HelpService', ['$translate', function ($translate) {
			var helpService = {};
			helpService.currentHelp = {};

			helpService.updateState = function(state) {
				helpService.currentHelp = {
					name: $translate.instant(state.current.name),
					helpDesc: '',
					helpUrl: 'http://help.teleopti.com/TeleoptiWFM/' + state.current.name
				}
			};

			return helpService;
		}
	]);
})();