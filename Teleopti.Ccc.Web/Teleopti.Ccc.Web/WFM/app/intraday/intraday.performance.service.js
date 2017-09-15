(function() {
    'use strict';
    angular.module('wfm.intraday').service('intradayPerformanceService', [
        '$filter',
        'intradayService',
        '$translate',
        function($filter, intradayService, $translate) {
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
            service.setPerformanceData = function(result, showEsl, showEmailSkill, isToday, showAbandonRate) {
                clearData();
				performanceData.averageSpeedOfAnswerObj.series = result.DataSeries.AverageSpeedOfAnswer;
				performanceData.showAbandonRate = showAbandonRate;
				if(showAbandonRate !== false){
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
                    summaryAbandonedRate: showAbandonRate!=false ? $filter('number')(result.Summary.AbandonRate * 100, 1) : undefined,
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
                    function(value, key) {
                        this.push($filter('date')(value, 'shortTime'));
                    },
                    performanceData.timeSeries
                );

                if (performanceData.timeSeries[0] != 'x') {
                    performanceData.timeSeries.splice(0, 0, 'x');
                }

                performanceData.hasMonitorData = result.PerformanceHasData;
                performanceData.currentInterval = [];

                if (isToday) {
                    getCurrent();
                }
                service.initPerformanceChart();
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

	            request = intradayService.getSkillMonitorPerformance
		            .query({
			            id: selectedItem.Id
					});

                    request.$promise.then(
                        function(result) {
                            performanceData.waitingForData = false;
                            service.setPerformanceData(
								result, 
								toggles.showEsl, 
								toggles.showEmailSkill, 
								true,
								selectedItem.ShowAbandonRate);
                        },
                        function(error) {
                            performanceData.hasMonitorData = false;
                        }
                    );
            };

            service.pollSkillDataByDayOffset = function(selectedItem, toggles, dayOffset) {
                performanceData.waitingForData = true;
				service.checkMixedArea(selectedItem);
	            cancelPendingRequest();

	            request = intradayService.getSkillMonitorPerformanceByDayOffset
		            .query({
			            id: selectedItem.Id,
			            dayOffset: dayOffset
					});

                request.$promise.then(
                        function(result) {
                            performanceData.waitingForData = false;
                            service.setPerformanceData(
                                result,
                                toggles.showEsl,
                                toggles.showEmailSkill,
								dayOffset === 0,
								selectedItem.ShowAbandonRate
                            );
                        },
                        function(error) {
                            performanceData.hasMonitorData = false;
                        }
                    );
            };

            service.pollSkillAreaData = function(selectedItem, toggles) {
                performanceData.waitingForData = true;
				service.checkMixedArea(selectedItem);
				
				var showAbandonRate = selectedItem.Skills.every(function(element, index, array){
					return element.ShowAbandonRate === true
				}) || !toggles.otherSkillsLikeEmail;
				
	            cancelPendingRequest();

	            request = intradayService.getSkillAreaMonitorPerformance
		            .query({
			            id: selectedItem.Id
					});

	            request.$promise.then(
                        function(result) {
                            performanceData.waitingForData = false;
                            service.setPerformanceData(result, toggles.showEsl, toggles.showEmailSkill && !toggles.otherSkillsLikeEmail, true, showAbandonRate);
                        },
                        function(error) {
                            performanceData.hasMonitorData = false;
                        }
                    );
            };

            service.pollSkillAreaDataByDayOffset = function(selectedItem, toggles, dayOffset) {
                performanceData.waitingForData = true;
				service.checkMixedArea(selectedItem);

				var showAbandonRate = selectedItem.Skills.every(function(element, index, array){
					return element.ShowAbandonRate === true
				}) || !toggles.otherSkillsLikeEmail;

	            cancelPendingRequest();
	            request = intradayService.getSkillAreaMonitorPerformanceByDayOffset
		            .query({
			            id: selectedItem.Id,
			            dayOffset: dayOffset
		            });
	            request.$promise.then(
                        function(result) {
                            performanceData.waitingForData = false;
                            service.setPerformanceData(result, toggles.showEsl, toggles.showEmailSkill && !toggles.otherSkillsLikeEmail, dayOffset === 0, showAbandonRate);
                        },
                        function(error) {
                            performanceData.hasMonitorData = false;
                        }
                    );
            };

            service.checkMixedArea = function(selectedItem) {
                //If multiskill
                if (selectedItem.Skills) {
                    function findEmail(area) {
                        return area.SkillType === 'SkillTypeEmail';
                    }
                    mixedArea = selectedItem.Skills.find(findEmail);
                } else {
                    mixedArea = selectedItem.SkillType === 'SkillTypeEmail';
                }
            };

            service.initPerformanceChart = function() {
                service.performanceChart = c3.generate({
                    bindto: '#performanceChart',
                    data: {
                        x: 'x',
                        columns: [
                            performanceData.timeSeries,
                            performanceData.averageSpeedOfAnswerObj.series,
                            performanceData.abandonedRateObj.series,
                            performanceData.serviceLevelObj.series,
                            performanceData.estimatedServiceLevelObj.series,
                            performanceData.currentInterval
                        ],
                        hide: hiddenArray,
                        type: 'line',
                        types: {
                            Current: 'bar'
                        },
                        colors: {
                            ASA: '#0099FF',
                            Abandoned_rate: '#E91E63',
                            Service_level: '#FB8C00',
                            ESL: '#F488C8'
                        },
                        names: {
                            ASA: $translate.instant('AverageSpeedOfAnswer') + ' ←',
                            Abandoned_rate: $translate.instant('AbandonedRate') + ' →',
                            Service_level: $translate.instant('ServiceLevel') + ' →',
                            ESL: $translate.instant('ESL') + ' →'
                        },
                        axes: {
                            Service_level: 'y2',
                            Abandoned_rate: 'y2',
                            ESL: 'y2'
                        }
                    },
                    axis: {
                        x: {
                            label: {
                                text: $translate.instant('SkillTypeTime'),
                                position: 'outer-center'
                            },
                            type: 'category',
                            tick: {
                                culling: {
                                    max: 24
                                },
                                fit: true,
                                centered: true,
                                multiline: false
                            }
                        },
                        y: {
                            label: {
                                text: $translate.instant('SecondShort'),
                                position: 'outer-middle'
                            },
                            tick: {
                                format: d3.format('.1f')
                            }
                        },
                        y2: {
                            label: {
                                text: $translate.instant('%'),
                                position: 'outer-middle'
                            },
                            show: true,
                            tick: {
                                format: d3.format('.1f')
                            }
                        }
                    },
                    legend: {
                        item: {
                            onclick: function(id) {
                                if (hiddenArray.indexOf(id) > -1) {
                                    hiddenArray.splice(hiddenArray.indexOf(id), 1);
                                } else {
                                    hiddenArray.push(id);
                                }
                                service.initPerformanceChart();
                            }
                        }
                    },
                    transition: {
                        duration: 500
                    }
                });
            };

            service.initPerformanceChart();

            return service;
        }
    ]);
})();
