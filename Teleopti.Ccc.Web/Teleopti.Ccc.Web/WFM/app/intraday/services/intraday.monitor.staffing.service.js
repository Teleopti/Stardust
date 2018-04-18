(function() {
	'use strict';
	angular
		.module('wfm.intraday')
		.service('intradayMonitorStaffingService', [
			'$filter',
			'intradayService',
			'SkillGroupSvc',
			'$translate',
			'$q',
			'$log',
			intradayMonitorStaffingService
		]);

	function intradayMonitorStaffingService($filter, intradayService, SkillGroupSvc, $translate, $q, $log) {
		var service = {
			staffingChart: {}
		};

		var staffingData = {
			forecastedStaffing: {
				max: {},
				series: [],
				updatedSeries: []
			},
			hasMonitorData: false,
			waitingForData: false,
			hasEmailSkill: false,
			showReforecastedAgents: true,
			timeSeries: [],
			actualStaffingSeries: [],
			currentInterval: [],
			scheduledStaffing: [],
			error: {}
		};

		var hiddenArray = [];
		var mixedArea = false;

		var setStaffingData = function(result, showOptimalStaffing, showReforecastedAgents) {
			clearData();

			if (result.DataSeries === null) return staffingData;
			staffingData.forecastedStaffing.series = result.DataSeries.ForecastedStaffing;

			staffingData.hasEmailSkill = mixedArea;
			staffingData.showReforecastedAgents = showReforecastedAgents !== false;
			if (showReforecastedAgents !== false && !mixedArea)
				staffingData.forecastedStaffing.updatedSeries = result.DataSeries.UpdatedForecastedStaffing;

			if (showOptimalStaffing) staffingData.actualStaffingSeries = result.DataSeries.ActualStaffing;

			staffingData.scheduledStaffing = result.DataSeries.ScheduledStaffing;

			angular.forEach(
				result.DataSeries.Time,
				function(value) {
					this.push($filter('date')(value, 'shortTime'));
				},
				staffingData.timeSeries
			);

			if (staffingData.timeSeries[0] !== 'x') {
				staffingData.timeSeries.splice(0, 0, 'x');
			}

			staffingData.hasMonitorData = result.StaffingHasData;
			var forecastedStaffingMax = Math.max.apply(Math, staffingData.forecastedStaffing.series);
			var updatedForecastedStaffingMax = Math.max.apply(Math, result.DataSeries.UpdatedForecastedStaffing);
			if (forecastedStaffingMax > updatedForecastedStaffingMax) {
				staffingData.forecastedStaffing.max = forecastedStaffingMax;
			} else {
				staffingData.forecastedStaffing.max = updatedForecastedStaffingMax;
			}

			staffingData.forecastedStaffing.series.splice(0, 0, 'Forecasted_staffing');
			staffingData.forecastedStaffing.updatedSeries.splice(0, 0, 'Updated_forecasted_staffing');
			staffingData.actualStaffingSeries.splice(0, 0, 'Actual_staffing');
			staffingData.scheduledStaffing.splice(0, 0, 'Scheduled_staffing');

			return staffingData;
		};

		var clearData = function() {
			staffingData.hasEmailSkill = false;
			staffingData.timeSeries = [];
			staffingData.forecastedStaffing.series = [];
			staffingData.forecastedStaffing.updatedSeries = [];
			staffingData.actualStaffingSeries = [];
			staffingData.scheduledStaffing = [];
			staffingData.scheduledStaffing.series = [];
			staffingData.actualStaffingSeries = [];
			staffingData.error = {};
		};

		var request;
		function cancelPendingRequest() {
			if (request) {
				request.$cancelRequest('cancel');
			}
		}

		service.pollSkillDataByDayOffset = function(selectedItem, toggles, dayOffset, gotData) {
			staffingData.waitingForData = true;
			service.checkMixedArea(selectedItem);

			cancelPendingRequest();
			request = intradayService.getSkillStaffingDataByDayOffset.query({
				id: selectedItem.Id,
				dayOffset: dayOffset
			});

			request.$promise.then(
				function(result) {
					staffingData.waitingForData = false;
					setStaffingData(result, dayOffset <= 0, selectedItem.ShowReforecastedAgents);
					gotData(staffingData);
				},
				function() {
					staffingData.hasMonitorData = false;
					gotData(staffingData);
				}
			);
		};

		service.pollSkillAreaDataByDayOffset = function(selectedItem, toggles, dayOffset, gotData) {
			staffingData.waitingForData = true;
			service.checkMixedArea(selectedItem);

			var showReforecastedAgents = selectedItem.Skills.every(function(element) {
				return element.ShowReforecastedAgents === true;
			});

			cancelPendingRequest();

			request = intradayService.getSkillAreaStaffingDataByDayOffset.query({
				id: selectedItem.Id,
				dayOffset: dayOffset
			});

			request.$promise.then(
				function(result) {
					staffingData.waitingForData = false;
					setStaffingData(result, dayOffset <= 0, showReforecastedAgents);
					gotData(staffingData);
				},
				function(error) {
					staffingData.hasMonitorData = false;
					staffingData.error = error;
					gotData(staffingData);
				}
			);
		};

		service.checkMixedArea = function(selectedItem) {
			//If multiskill
			if (selectedItem.Skills) {
				mixedArea = selectedItem.Skills.find(function(area) {
					return area.SkillType === 'SkillTypeEmail';
				});
			} else {
				mixedArea = selectedItem.SkillType === 'SkillTypeEmail';
			}
		};

		return service;
	}
})();
