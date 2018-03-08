﻿(function (angular) {
	'use strict';
	angular.module('wfm.teamSchedule').component('teamsExportSchedule',
			{
				controller: TeamsExportScheduleCtrl,
				templateUrl: 'app/teamSchedule/html/exportSchedule.html',
				controllerAs: 'vm'
			});

	TeamsExportScheduleCtrl.$inject = ['$state', '$timeout', '$scope', 'groupPageService', 'exportScheduleService', 'NoticeService','serviceDateFormatHelper'];
	function TeamsExportScheduleCtrl($state, $timeout, $scope, groupPageService, exportScheduleService, NoticeService, serviceDateFormatHelper) {
		var vm = this;
		vm.configuration = {
			period:{
				startDate : new Date(),
				endDate : new Date()
			}
		};
		vm.periodPickerType = 'popup';
		vm.scenarios = [];
		vm.optionalColumns = [];
		vm.availableGroups = { BusinessHierarchy: [], GroupPages: [] };
		vm.timezones = [];
		vm.configuration.selectedGroups = {
			mode: 'BusinessHierarchy',
			groupIds: [],
			groupPageId: ''
		};
		vm.dateRangeCustomValidators = [{
			key: 'lessThan31Days',
			message: 'ExportSchedulesMaximumDays',
			validate: function(start, end) {
				var maxEndDate = moment(start).add(30, 'days');
				return moment(end).isSame(maxEndDate) || moment(end).isBefore(maxEndDate);
			}
		}];
		vm.isExporting = false;

		vm.changeTimezone = function (timezone) {
			vm.configuration.timezoneId = timezone;
		};

		vm.isOptionalColDisabled = function(optionalColumnId) {
			var result = vm.configuration.optionalColumnIds &&
				vm.configuration.optionalColumnIds.length >= 3 && 
				vm.configuration.optionalColumnIds.indexOf(optionalColumnId) === -1;
			return result;
		};
		
		vm.isApplyEnabled = function() {
			return $scope.exportScheduleForm.$valid && vm.configuration.selectedGroups && vm.configuration.selectedGroups.groupIds && vm.configuration.selectedGroups.groupIds.length > 0;
		};
		
		vm.onPeriodChanged = function () {
			vm.getGroupPagesAsync();
		};

		vm.getGroupPagesAsync = function () {
			if (vm.configuration.period) {
				var startDate = serviceDateFormatHelper.getDateOnly(vm.configuration.period.startDate);
				var endDate = serviceDateFormatHelper.getDateOnly(vm.configuration.period.endDate);
				groupPageService.fetchAvailableGroupPages(startDate, endDate).then(function (data) {
					vm.availableGroups = data;
					if (data.LogonUserTeamId) {
						vm.configuration.selectedGroups = {
							mode: 'BusinessHierarchy',
							groupIds: [vm.availableGroups.LogonUserTeamId]
						};
					}
				});
			}
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
			vm.isExporting = true;
			exportScheduleService.startExport(vm.configuration).then(function(response) {
			        vm.isExporting = false;
				var failReason = response.headers()['message'];
				if(failReason && failReason.length > 0){
					NoticeService.error(failReason, null, true);
				}
				else{
					saveData(response.data);
				}
				
			});
		}

		vm.$onInit = function () {
			vm.getGroupPagesAsync();
			vm.getScenariosAsync();
			vm.getTimezonesAsync();
			vm.getOptionalColumnsAsync();
		}

		function saveData(data) {
			var blob = new Blob([data]);
			vm.exporting = false;
			saveAs(blob, 'TeamsExportedSchedules' + moment().format('YYYY-MM-DD') + '.xlsx');
		}
	}
})(angular);