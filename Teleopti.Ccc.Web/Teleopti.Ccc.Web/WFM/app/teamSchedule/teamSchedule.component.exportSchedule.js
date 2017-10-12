(function (angular) {
	'use strict';
	angular.module('wfm.teamSchedule').component('teamsExportSchedule', {
		controller: TeamsExportScheduleCtrl,
		templateUrl: 'app/teamSchedule/html/exportSchedule.html',
		controllerAs: 'vm'
	});

	TeamsExportScheduleCtrl.$inject = ['groupPageService', 'exportScheduleService'];
	function TeamsExportScheduleCtrl(groupPageService, exportScheduleService) {
		var vm = this;
		vm.configuration = {};
		vm.scenarios = [];
		vm.optionalColumns = [];
		vm.availableGroups = { BusinessHierarchy: [], GroupPages: [] };
		vm.selectedGroups = {
			mode: 'BusinessHierarchy',
			groupIds: [],
			groupPageId: ''
		};

		vm.maxEndDate = moment(new Date()).add(31, 'days').toDate();
		vm.maxStartDate = moment(new Date()).add(-31, 'days').toDate();
		var startDate = new Date();
		var endDate = new Date();
		Object.defineProperty(vm.configuration, 'startDate',
			{
				get: function () {
					return startDate;
				},
				set: function (value) {
					startDate = value;
					vm.maxEndDate = moment(startDate).add(31, 'days').toDate();
				}
			});

		Object.defineProperty(vm.configuration, 'endDate',
			{
				get: function () {
					return endDate;
				},
				set: function (value) {
					endDate = value;
					vm.maxStartDate = moment(endDate).add(-31, 'days').toDate();
				}
			});

		vm.getGroupPagesAsync = function () {
			var startDate = moment(vm.configuration.startDate).format('YYYY-MM-DD');
			var endDate = moment(vm.configuration.endDate).format('YYYY-MM-DD');
			groupPageService.fetchAvailableGroupPages(startDate, endDate).then(function (data) {
				vm.availableGroups = data;
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

		}
		vm.startExport = function () { }

		vm.$onInit = function () {
			vm.getGroupPagesAsync();
			vm.getScenariosAsync();
			vm.getTimezonesAsync();
			vm.getOptionalColumnsAsync();
		}
	}
})(angular);