(function (angular) {
	'use strict';
	angular.module('wfm.teamSchedule').component('teamsExportSchedule', {
		controller: TeamsExportScheduleCtrl,
		templateUrl: 'app/teamSchedule/html/exportSchedule.html',
		controllerAs: 'vm'
	});

	TeamsExportScheduleCtrl.$inject = ['groupPageService','exportScheduleService'];
	function TeamsExportScheduleCtrl( groupPageService, exportScheduleService) {
		var vm = this;
		vm.configuration = {};
		vm.scenarios = [];
		vm.availableGroups = {BusinessHierarchy:[],GroupPages:[]};
		vm.selectedGroups = {
			mode: 'BusinessHierarchy',
			groupIds: [],
			groupPageId: ''
		};

		vm.getGroupPagesAsync = function (date) {
			var dateStr = moment(date).format('YYYY-MM-DD');
			groupPageService.fetchAvailableGroupPages(dateStr, dateStr).then(function (data) {
				vm.availableGroups = data;
			});
		};

		vm.getScenariosAsync = function () {
			exportScheduleService.getScenarioData().then(function (data) {
				vm.scenarios = data;
			});
		};
		
		vm.$onInit = function () {
			vm.getGroupPagesAsync(new Date());
			vm.getScenariosAsync();
		}
	}
})(angular);