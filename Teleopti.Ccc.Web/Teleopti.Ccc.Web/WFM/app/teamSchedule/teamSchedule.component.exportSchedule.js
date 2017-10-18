(function (angular) {
	'use strict';
	angular.module('wfm.teamSchedule').component('teamsExportSchedule',
			{
				controller: TeamsExportScheduleCtrl,
				templateUrl: 'app/teamSchedule/html/exportSchedule.html',
				controllerAs: 'vm'
			});

	TeamsExportScheduleCtrl.$inject = ['$state','$timeout', '$scope', 'groupPageService', 'exportScheduleService'];
	function TeamsExportScheduleCtrl($state, $timeout, $scope, groupPageService, exportScheduleService) {
		var vm = this;
		vm.configuration = {
			startDate : new Date(),
			endDate : new Date()
		};
		vm.scenarios = [];
		vm.optionalColumns = [];
		vm.availableGroups = { BusinessHierarchy: [], GroupPages: [] };
		vm.selectedGroups = {
			mode: 'BusinessHierarchy',
			groupIds: [],
			groupPageId: ''
		};

		vm.maxEndDate = moment(new Date()).add(30, 'days').toDate();
		vm.onStartDateChanged = function() {
			vm.maxEndDate = moment(vm.configuration.startDate).add(30, 'days').toDate();
			$timeout(function() {
				vm.configuration.endDate = moment(vm.configuration.endDate).toDate();
			});
		}

		vm.isOptionalColDisabled = function(optionalColumnId) {
			var result = vm.configuration.optionalColumnIds &&
				vm.configuration.optionalColumnIds.length >= 3 && 
				vm.configuration.optionalColumnIds.indexOf(optionalColumnId) === -1;
			return result;
		}

		vm.isApplyEnabled = function() {
			return $scope.exportScheduleForm.$valid && vm.configuration.selectedGroups && vm.configuration.selectedGroups.groupIds && vm.configuration.selectedGroups.groupIds.length > 0;
		}

		vm.getGroupPagesAsync = function () {
			var startDate = moment(vm.configuration.startDate).format('YYYY-MM-DD');
			var endDate = moment(vm.configuration.endDate).format('YYYY-MM-DD');
			groupPageService.fetchAvailableGroupPages(startDate, endDate).then(function (data) {
				vm.availableGroups = data;
				if (data.LogonUserTeamId) {
					vm.configuration.selectedGroups = {
						mode: 'BusinessHierarchy',
						groupIds: [vm.availableGroups.LogonUserTeamId]
					};
				}
			});
		};

		vm.getScenariosAsync = function () {
			exportScheduleService.getScenarioData().then(function (data) {
				vm.scenarios = data;
				if (!!data.length) {
					vm.configuration.scenarioId = data[0].Id;
				}
			});
		};

		vm.getTimezonesAsync = function () {
			exportScheduleService.getTimezonesData().then(function (data) {
				vm.timezones = data.Timezones;
				vm.configuration.timezoneId = data.DefaultTimezone;
			});
		}

		vm.getOptionalColumnsAsync = function () {
			exportScheduleService.getOptionalColumnsData().then(function (data) {
				vm.optionalColumns = data;
			});
		}

		vm.gotoDayView = function () {
			$state.go('teams.dayView');
		}
		vm.startExport = function() {
			exportScheduleService.startExport(vm.configuration).then(function(data) {
				
			});
		}

		vm.$onInit = function () {
			vm.getGroupPagesAsync();
			vm.getScenariosAsync();
			vm.getTimezonesAsync();
			vm.getOptionalColumnsAsync();
		}
	}
})(angular);