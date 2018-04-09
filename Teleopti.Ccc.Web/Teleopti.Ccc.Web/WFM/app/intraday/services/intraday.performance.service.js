(function() {
	'use strict';
	angular
		.module('wfm.intraday')
		.service('intradayPerformanceService', [
			'$filter',
			'intradayService',
			'$translate',
			intradayPerformanceService
		]);

	function intradayPerformanceService($filter, intradayService, $translate) {
		var service = {
			performanceChart: {}
		};

		var performanceData = {
			averageSpeedOfAnswerObj: {
				series: {},
				max: {}
			},
			abandonedRateObj: {
				series: {},
				max: {}
			},
			serviceLevelObj: {
				series: {},
				max: {}
			},
			estimatedServiceLevelObj: {
				series: {},
				max: {}
			},
			summary: {},
			hasMonitorData: false,
			hasEmailSkill: false,
			showAbandonRate: true,
			waitingForData: false,
			timeSeries: [],
			currentInterval: []
		};

		var hiddenArray = [];
		var intervalStart;
		var mixedArea = false;

		var setPerformanceData = function(result, showEsl, showEmailSkill, isToday, showAbandonRate) {
			clearData();
			performanceData.averageSpeedOfAnswerObj.series = result.DataSeries.AverageSpeedOfAnswer;
			performanceData.showAbandonRate = showAbandonRate;
			if (showAbandonRate !== false) {
				performanceData.abandonedRateObj.series = result.DataSeries.AbandonedRate;
			}
			performanceData.serviceLevelObj.series = result.DataSeries.ServiceLevel;
			if (showEsl) performanceData.estimatedServiceLevelObj.series = result.DataSeries.EstimatedServiceLevels;

			performanceData.latestActualInterval =
				$filter('date')(result.LatestActualIntervalStart, 'shortTime') +
				' - ' +
				$filter('date')(result.LatestActualIntervalEnd, 'shortTime');
			intervalStart = $filter('date')(result.LatestActualIntervalStart, 'shortTime');

			performanceData.averageSpeedOfAnswerObj.max = Math.max.apply(
				Math,
				performanceData.averageSpeedOfAnswerObj.series
			);
			performanceData.abandonedRateObj.max = Math.max.apply(Math, performanceData.abandonedRateObj.series);
			performanceData.serviceLevelObj.max = Math.max.apply(Math, performanceData.serviceLevelObj.series);
			performanceData.estimatedServiceLevelObj.max = Math.max.apply(
				Math,
				performanceData.estimatedServiceLevelObj.series
			);

			performanceData.averageSpeedOfAnswerObj.series.splice(0, 0, 'ASA');
			performanceData.abandonedRateObj.series.splice(0, 0, 'Abandoned_rate');
			performanceData.serviceLevelObj.series.splice(0, 0, 'Service_level');
			performanceData.estimatedServiceLevelObj.series.splice(0, 0, 'ESL');

			performanceData.summary = {
				summaryAbandonedRate:
					showAbandonRate !== false ? $filter('number')(result.Summary.AbandonRate * 100, 1) : undefined,
				summaryServiceLevel: $filter('number')(result.Summary.ServiceLevel * 100, 1),
				summaryAverageSpeedOfAnswer: $filter('number')(result.Summary.AverageSpeedOfAnswer, 1)
			};

			if (showEsl)
				performanceData.summary.summaryEstimatedServiceLevel = $filter('number')(
					result.Summary.EstimatedServiceLevel,
					1
				);

			if (showEmailSkill && mixedArea) {
				performanceData.abandonedRateObj.series = [];
				performanceData.hasEmailSkill = true;
			}

			angular.forEach(
				result.DataSeries.Time,
				function(value) {
					this.push($filter('date')(value, 'shortTime'));
				},
				performanceData.timeSeries
			);

			if (performanceData.timeSeries[0] !== 'x') {
				performanceData.timeSeries.splice(0, 0, 'x');
			}

			performanceData.hasMonitorData = result.PerformanceHasData;
			performanceData.currentInterval = [];

			if (isToday) {
				getCurrent();
			}
			return performanceData;
		};

		service.getData = function() {
			return performanceData;
		};

		var getCurrent = function() {
			for (var i = 0; i < performanceData.timeSeries.length; i++) {
				if (performanceData.timeSeries[i] === intervalStart) {
					performanceData.currentInterval[i] = performanceData.averageSpeedOfAnswerObj.max;
				} else {
					performanceData.currentInterval[i] = null;
				}
			}
			performanceData.currentInterval[0] = 'Current';
		};

		var clearData = function() {
			performanceData.hasEmailSkill = false;
			performanceData.timeSeries = [];
			performanceData.averageSpeedOfAnswerObj.series = [];
			performanceData.abandonedRateObj.series = [];
			performanceData.serviceLevelObj.series = [];
			performanceData.estimatedServiceLevelObj.series = [];
		};

		var request;
		function cancelPendingRequest() {
			if (request) {
				request.$cancelRequest('cancel');
			}
		}

		service.pollSkillData = function(selectedItem, toggles) {
			performanceData.waitingForData = true;
			service.checkMixedArea(selectedItem);
			cancelPendingRequest();

			request = intradayService.getSkillMonitorPerformance.query({
				id: selectedItem.Id
			});

			request.$promise.then(
				function(result) {
					performanceData.waitingForData = false;
					setPerformanceData(
						result,
						toggles['Wfm_Intraday_ESL_41827'], //.showEsl,
						toggles['Wfm_Intraday_SupportSkillTypeEmail_44002'], //.showEmailSkill,
						true,
						selectedItem.ShowAbandonRate
					);
				},
				function() {
					performanceData.hasMonitorData = false;
				}
			);
		};

		service.pollSkillDataByDayOffset = function(selectedItem, toggles, dayOffset, gotData) {
			performanceData.waitingForData = true;
			service.checkMixedArea(selectedItem);
			cancelPendingRequest();

			request = intradayService.getSkillMonitorPerformanceByDayOffset.query({
				id: selectedItem.Id,
				dayOffset: dayOffset
			});

			request.$promise.then(
				function(result) {
					performanceData.waitingForData = false;
					setPerformanceData(
						result,
						toggles['Wfm_Intraday_ESL_41827'],
						toggles['Wfm_Intraday_SupportSkillTypeEmail_44002'],
						dayOffset === 0,
						selectedItem.ShowAbandonRate
					);
					gotData(performanceData);
				},
				function() {
					performanceData.hasMonitorData = false;
					gotData(performanceData);
				}
			);
		};

		service.pollSkillAreaDataByDayOffset = function(selectedItem, toggles, dayOffset, gotData) {
			performanceData.waitingForData = true;
			service.checkMixedArea(selectedItem);

			var showAbandonRate =
				selectedItem.Skills.every(function(element) {
					return element.ShowAbandonRate === true;
				}) || !toggles['WFM_Intraday_SupportOtherSkillsLikeEmail_44026'];

			cancelPendingRequest();
			request = intradayService.getSkillAreaMonitorPerformanceByDayOffset.query({
				id: selectedItem.Id,
				dayOffset: dayOffset
			});
			request.$promise.then(
				function(result) {
					performanceData.waitingForData = false;
					setPerformanceData(
						result,
						toggles['Wfm_Intraday_ESL_41827'],
						toggles['Wfm_Intraday_SupportSkillTypeEmail_44002'] &&
							!toggles['WFM_Intraday_SupportOtherSkillsLikeEmail_44026'],
						dayOffset === 0,
						showAbandonRate
					);
					gotData(performanceData);
				},
				function() {
					performanceData.hasMonitorData = false;
					gotData(performanceData);
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
