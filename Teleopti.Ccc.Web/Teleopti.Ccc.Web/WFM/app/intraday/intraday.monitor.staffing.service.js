(function() {
    'use strict';
    angular.module('wfm.intraday').service('intradayMonitorStaffingService', [
        '$filter',
        'intradayService',
        '$translate',
        function($filter, intradayService, $translate) {
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
                timeSeries: [],
                actualStaffingSeries: [],
                currentInterval: [],
                scheduledStaffing: []
            };

            var hiddenArray = [];
			var mixedArea = null;

            service.setStaffingData = function(result, showOptimalStaffing, showScheduledStaffing) {
                clearData();

                staffingData.timeSeries = [];
                staffingData.forecastedStaffing.series = [];
                staffingData.forecastedStaffing.updatedSeries = [];
                staffingData.actualStaffingSeries = [];
                staffingData.scheduledStaffing = [];

                if (result.DataSeries == null) return staffingData;
                staffingData.forecastedStaffing.series = result.DataSeries.ForecastedStaffing;
				if (!showEmailSkill || !mixedArea){
                staffingData.forecastedStaffing.updatedSeries = result.DataSeries.UpdatedForecastedStaffing;
				}else{
					staffingData.hasEmailSkill= true;
				}


                if (showScheduledStaffing) staffingData.scheduledStaffing = result.DataSeries.ScheduledStaffing;

                angular.forEach(
                    result.DataSeries.Time,
                    function(value, key) {
                        this.push($filter('date')(value, 'shortTime'));
                    },
                    staffingData.timeSeries
                );

                if (staffingData.timeSeries[0] != 'x') {
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

                if (service.staffingChart.data().length <= 0) {
                    service.initStaffingChart();
                }

                service.loadStaffingChart(staffingData);
                return staffingData;
            };

            service.getData = function() {
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

            service.pollSkillData = function(selectedItem, toggles) {
                staffingData.waitingForData = true;
				if (selectedItem.SkillType === 'SkillTypeEmail') {
					mixedArea = selectedItem;
				} else {
					mixedArea = false;
				}
						return service.setStaffingData(result, toggles.showOptimalStaffing, toggles.showScheduledStaffing, toggles.showEmailSkill);

				function findEmail(area) {
					return area.SkillType === 'SkillTypeEmail';
				}
				mixedArea = selectedItem.Skills.find(findEmail);

                intradayService.getSkillStaffingData
                    .query({
                        id: selectedItem.Id
                    })
                    .$promise.then(
                        function(result) {
                            staffingData.waitingForData = false;
                            return service.setStaffingData(
                                result,
                                toggles.showOptimalStaffing,
                                toggles.showScheduledStaffing
                            );
                        },
                        function(error) {
                            staffingData.hasMonitorData = false;
                        }
                    );
            };

            service.pollSkillAreaData = function(selectedItem, toggles) {
                staffingData.waitingForData = true;
                intradayService.getSkillAreaStaffingData
                    .query({
                        id: selectedItem.Id
                    })
                    .$promise.then(
                        function(result) {
                            staffingData.waitingForData = false;
                            return service.setStaffingData(
                                result,
                                toggles.showOptimalStaffing,
                                toggles.showScheduledStaffing
                            );
                        },
                        function(error) {
                            staffingData.hasMonitorData = false;
                        }
                    );
            };

            service.pollSkillDataByDayOffset = function(selectedItem, toggles, dayOffset) {
                staffingData.waitingForData = true;
                intradayService.getSkillStaffingDataByDayOffset
                    .query({
                        id: selectedItem.Id,
                        dayOffset: dayOffset
                    })
                    .$promise.then(
                        function(result) {
                            staffingData.waitingForData = false;
                            return service.setStaffingData(
                                result,
                                toggles.showOptimalStaffing,
                                toggles.showScheduledStaffing
                            );
                        },
                        function(error) {
                            staffingData.hasMonitorData = false;
                        }
                    );
            };

            service.pollSkillAreaDataByDayOffset = function(selectedItem, toggles, dayOffset) {
                staffingData.waitingForData = true;
                intradayService.getSkillAreaStaffingDataByDayOffset
                    .query({
                        id: selectedItem.Id,
                        dayOffset: dayOffset
                    })
                    .$promise.then(
                        function(result) {
                            staffingData.waitingForData = false;
                            return service.setStaffingData(
                                result,
                                toggles.showOptimalStaffing,
                                toggles.showScheduledStaffing
                            );
                        },
                        function(error) {
                            staffingData.hasMonitorData = false;
                        }
                    );
            };

            service.loadStaffingChart = function(sData) {
				if(!sData) return;
                service.staffingChart.load({
                    columns: [
                        sData.timeSeries,
                        sData.forecastedStaffing.series,
                        sData.forecastedStaffing.updatedSeries,
                        sData.actualStaffingSeries,
                        sData.scheduledStaffing
                    ]
                });
            };

            service.initStaffingChart = function() {
                service.staffingChart = c3.generate({
                    bindto: '#staffingChart',
                    data: {
                        x: 'x',
                        columns: [],
                        type: 'line',
                        hide: hiddenArray,
                        names: {
                            Forecasted_staffing: $translate.instant('ForecastedStaff') + ' ←',
                            Updated_forecasted_staffing: $translate.instant('ReforecastedStaff') + ' ←',
                            Actual_staffing: $translate.instant('RequiredStaff') + ' ←',
                            Scheduled_staffing: $translate.instant('ScheduledStaff') + ' ←'
                        },
                        colors: {
                            Forecasted_staffing: '#0099FF',
                            Updated_forecasted_staffing: '#E91E63',
                            Actual_staffing: '#FB8C00',
                            Scheduled_staffing: '#F488C8'
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
                                text: $translate.instant('Agents'),
                                position: 'outer-middle'
                            },
                            tick: {
                                format: d3.format('.1f')
                            }
                        },
                        y2: {
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
                                service.initStaffingChart();
                                service.loadStaffingChart(staffingData);
                            }
                        }
                    },
                    transition: {
                        duration: 500
                    }
                });
            };

            service.initStaffingChart();
            service.loadStaffingChart(staffingData);

            return service;
        }
    ]);
})();
