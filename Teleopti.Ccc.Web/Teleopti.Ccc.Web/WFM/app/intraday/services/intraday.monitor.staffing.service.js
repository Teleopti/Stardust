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
			scheduledStaffing: []
		};

		var hiddenArray = [];
		var mixedArea = false;

		var setStaffingData = function(
			result,
			showOptimalStaffing,
			showScheduledStaffing,
			showEmailSkill,
			showReforecastedAgents
		) {
			clearData();

			staffingData.timeSeries = [];
			staffingData.forecastedStaffing.series = [];
			staffingData.forecastedStaffing.updatedSeries = [];
			staffingData.actualStaffingSeries = [];
			staffingData.scheduledStaffing = [];

			if (result.DataSeries === null) return staffingData;
			staffingData.forecastedStaffing.series = result.DataSeries.ForecastedStaffing;

			staffingData.hasEmailSkill = showEmailSkill && mixedArea;
			staffingData.showReforecastedAgents = showReforecastedAgents !== false;
			if (showReforecastedAgents !== false && (!showEmailSkill || !mixedArea))
				staffingData.forecastedStaffing.updatedSeries = result.DataSeries.UpdatedForecastedStaffing;

			if (showOptimalStaffing) staffingData.actualStaffingSeries = result.DataSeries.ActualStaffing;

			if (showScheduledStaffing) staffingData.scheduledStaffing = result.DataSeries.ScheduledStaffing;

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
			staffingData.scheduledStaffing.series = [];
		};

		var request;
		function cancelPendingRequest() {
			if (request) {
				request.$cancelRequest('cancel');
			}
		}

		service.pollSkillData = function(selectedItem, toggles) {
			staffingData.waitingForData = true;
			cancelPendingRequest();

			service.checkMixedArea(selectedItem);
			request = intradayService.getSkillStaffingData.query({
				id: selectedItem.Id
			});

			request.$promise.then(
				function(result) {
					staffingData.waitingForData = false;
					return setStaffingData(
						result,
						toggles['Wfm_Intraday_OptimalStaffing_40921'],
						toggles['Wfm_Intraday_ScheduledStaffing_41476'],
						toggles['Wfm_Intraday_SupportSkillTypeEmail_44002'],
						selectedItem.ShowReforecastedAgents
					);
				},
				function() {
					staffingData.hasMonitorData = false;
				}
			);
		};

		service.pollSkillDataByDayOffset = function(selectedItem, toggles, dayOffset) {
			staffingData.waitingForData = true;
			service.checkMixedArea(selectedItem);

			cancelPendingRequest();
			request = intradayService.getSkillStaffingDataByDayOffset.query({
				id: selectedItem.Id,
				dayOffset: dayOffset
			});

			return $q(function(resolve) {
				request.$promise.then(
					function(result) {
						staffingData.waitingForData = false;
						setStaffingData(
							result,
							toggles['Wfm_Intraday_OptimalStaffing_40921'] && dayOffset <= 0,
							toggles['Wfm_Intraday_ScheduledStaffing_41476'],
							toggles['Wfm_Intraday_SupportSkillTypeEmail_44002'],
							selectedItem.ShowReforecastedAgents
						);
						resolve(staffingData);
					},
					function() {
						staffingData.hasMonitorData = false;
						resolve(staffingData);
					}
				);
			});
		};

		service.pollSkillAreaDataByDayOffset = function(selectedItem, toggles, dayOffset) {
			staffingData.waitingForData = true;
			service.checkMixedArea(selectedItem);

			var showReforecastedAgents =
				selectedItem.Skills.every(function(element) {
					return element.ShowReforecastedAgents === true;
				}) || !toggles['WFM_Intraday_SupportOtherSkillsLikeEmail_44026'];

			cancelPendingRequest();

			request = intradayService.getSkillAreaStaffingDataByDayOffset.query({
				id: selectedItem.Id,
				dayOffset: dayOffset
			});

			return $q(function(resolve, reject) {
				request.$promise.then(
					function(result) {
						staffingData.waitingForData = false;
						setStaffingData(
							result,
							toggles['Wfm_Intraday_OptimalStaffing_40921'] && dayOffset <= 0,
							toggles['Wfm_Intraday_ScheduledStaffing_41476'],
							toggles['Wfm_Intraday_SupportSkillTypeEmail_44002'],
							showReforecastedAgents
						);
						resolve(staffingData);
					},
					function() {
						staffingData.hasMonitorData = false;
						resolve(staffingData);
					}
				);
			});
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
