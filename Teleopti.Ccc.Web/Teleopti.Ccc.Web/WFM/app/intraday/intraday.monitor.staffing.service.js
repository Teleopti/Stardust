(function () {
    'use strict';
    angular.module('wfm.intraday').service('intradayMonitorStaffingService', [
        '$filter',
        'intradayService',
        '$translate',
        function ($filter, intradayService, $translate) {
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

            service.setStaffingData = function (result, showOptimalStaffing, showScheduledStaffing, showEmailSkill, showReforecastedAgents) {
                clearData();

                staffingData.timeSeries = [];
                staffingData.forecastedStaffing.series = [];
                staffingData.forecastedStaffing.updatedSeries = [];
                staffingData.actualStaffingSeries = [];
                staffingData.scheduledStaffing = [];

                if (result.DataSeries == null) return staffingData;
                staffingData.forecastedStaffing.series = result.DataSeries.ForecastedStaffing;

				staffingData.hasEmailSkill = (showEmailSkill && mixedArea);
				staffingData.showReforecastedAgents = showReforecastedAgents !== false;
				if(showReforecastedAgents !== false && (!showEmailSkill || !mixedArea)) 
					staffingData.forecastedStaffing.updatedSeries = result.DataSeries.UpdatedForecastedStaffing;

				if (showOptimalStaffing) staffingData.actualStaffingSeries = result.DataSeries.ActualStaffing;

                if (showScheduledStaffing) staffingData.scheduledStaffing = result.DataSeries.ScheduledStaffing;

                angular.forEach(
                    result.DataSeries.Time,
                    function (value, key) {
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

                service.initStaffingChart();

                return staffingData;
            };

            service.getData = function () {
                return staffingData;
            };

            var clearData = function () {
                staffingData.hasEmailSkill = false;
                staffingData.timeSeries = [];
                staffingData.forecastedStaffing.series = [];
                staffingData.forecastedStaffing.updatedSeries = [];
                staffingData.actualStaffingSeries = [];
                staffingData.scheduledStaffing.series = [];
            };

            service.pollSkillData = function (selectedItem, toggles) {
                staffingData.waitingForData = true;
	            service.checkMixedArea(selectedItem);
                intradayService.getSkillStaffingData
                    .query({
                        id: selectedItem.Id
                    })
                    .$promise.then(
                    function (result) {
                        staffingData.waitingForData = false;
                        return service.setStaffingData(
                            result,
                            toggles.showOptimalStaffing,
                            toggles.showScheduledStaffing,
                            toggles.showEmailSkill,
							selectedItem.ShowReforecastedAgents
                        );
                    },
                    function (error) {
                        staffingData.hasMonitorData = false;
                    }
                    );
            };

            service.pollSkillAreaData = function (selectedItem, toggles) {
                staffingData.waitingForData = true;
				service.checkMixedArea(selectedItem);
                intradayService.getSkillAreaStaffingData
                    .query({
                        id: selectedItem.Id
                    })
                    .$promise.then(
                    function (result) {
                        staffingData.waitingForData = false;
                        return service.setStaffingData(
                            result,
                            toggles.showOptimalStaffing,
                            toggles.showScheduledStaffing,
                            toggles.showEmailSkill,
							selectedItem.ShowReforecastedAgents
                        );
                    },
                    function (error) {
                        staffingData.hasMonitorData = false;
                    }
                    );
            };

            service.pollSkillDataByDayOffset = function (selectedItem, toggles, dayOffset) {
                staffingData.waitingForData = true;
	            service.checkMixedArea(selectedItem);
                intradayService.getSkillStaffingDataByDayOffset
                    .query({
                        id: selectedItem.Id,
                        dayOffset: dayOffset
                    })
                    .$promise.then(
                    function (result) {
                        staffingData.waitingForData = false;
                        return service.setStaffingData(
                            result,
                            toggles.showOptimalStaffing && dayOffset <= 0,
                            toggles.showScheduledStaffing,
							toggles.showEmailSkill,
							selectedItem.ShowReforecastedAgents
                        );
                    },
                    function (error) {
                        staffingData.hasMonitorData = false;
                    }
                    );
            };

            service.pollSkillAreaDataByDayOffset = function (selectedItem, toggles, dayOffset) {
                staffingData.waitingForData = true;
	            service.checkMixedArea(selectedItem);
                intradayService.getSkillAreaStaffingDataByDayOffset
                    .query({
                        id: selectedItem.Id,
                        dayOffset: dayOffset
                    })
                    .$promise.then(
                    function (result) {
                        staffingData.waitingForData = false;
                        return service.setStaffingData(
                            result,
							toggles.showOptimalStaffing && dayOffset <= 0,
                            toggles.showScheduledStaffing,
                            toggles.showEmailSkill,
							selectedItem.ShowReforecastedAgents
                        );
                    },
                    function (error) {
                        staffingData.hasMonitorData = false;
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
			}

            service.initStaffingChart = function () {
                service.staffingChart = c3.generate({
                    bindto: '#staffingChart',
                    data: {
                        x: 'x',
                        columns: [
                            staffingData.timeSeries,
                            staffingData.forecastedStaffing.series,
                            staffingData.forecastedStaffing.updatedSeries,
                            staffingData.actualStaffingSeries,
                            staffingData.scheduledStaffing
                        ],
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
                            onclick: function (id) {
                                if (hiddenArray.indexOf(id) > -1) {
                                    hiddenArray.splice(hiddenArray.indexOf(id), 1);
                                } else {
                                    hiddenArray.push(id);
                                }
                                service.initStaffingChart();
                            }
                        }
                    },
                    transition: {
                        duration: 500
                    }
                });
            };

            service.initStaffingChart();

            return service;
        }
    ]);
})();
