(function() {
	'use strict';
	var help = angular.module('wfm.help', []);

	help.controller('HelpController', [
		'$scope', 'HelpService', function ($scope, HelpService) {
			$scope.help = HelpService;
		}
	]);

	help.service('HelpService', [
		'$filter', function($filter) {
			var helpService = {};
			helpService.helpData = [];
			helpService.currentHelp = {};
			helpService.helpData.push({
				name: 'rta',
				helpDesc: 'This is the RTA view',
				helpUrl: 'http://www.teleopti.com/se/home.aspx'
			});
			helpService.stateName = '';
			helpService.updateState = function(stateName) {
				helpService.stateName = stateName;
				var result = $filter('filter')(helpService.helpData, { name: stateName });
				helpService.currentHelp = result.length > 0 ? result[0] : {};
			};

			return helpService;
		}
	]);
})();