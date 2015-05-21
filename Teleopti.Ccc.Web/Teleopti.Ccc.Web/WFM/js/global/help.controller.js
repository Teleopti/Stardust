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
				name: 'forecasting',
				helpDesc: 'This is the forecasting view',
				helpUrl: 'http://www.teleopti.com/se/home.aspx'
			},
			{
				name: 'forecasting.target',
				helpDesc: 'This is the advanced forecasting view',
				helpUrl: 'http://www.teleopti.com/se/home.aspx'
			},
			{
				name: 'people',
				helpDesc: 'This is the people view',
				helpUrl: 'http://www.teleopti.com/se/home.aspx'
			},
			{
				name: 'rta',
				helpDesc: 'This is the rta view',
				helpUrl: 'http://www.teleopti.com/se/home.aspx'
			},
			{
				name: 'outbound',
				helpDesc: 'This is the outbound view',
				helpUrl: 'http://www.teleopti.com/se/home.aspx'
			},
			{
				name: 'resourceplanner',
				helpDesc: 'This is the resourceplanner view',
				helpUrl: 'http://www.teleopti.com/se/home.aspx'
			},
			{
				name: 'permissions',
				helpDesc: 'This is the permissions view',
				helpUrl: 'http://www.teleopti.com/se/home.aspx'
			},
			{
				name: 'seatPlan',
				helpDesc: 'This is the seatPlan view',
				helpUrl: 'http://www.teleopti.com/se/home.aspx'
			},
			{
				name: 'seatMap',
				helpDesc: 'This is the seatMap view',
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