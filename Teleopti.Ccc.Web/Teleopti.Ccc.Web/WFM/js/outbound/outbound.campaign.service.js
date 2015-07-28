(function () {
    'use strict';

    angular.module('outboundServiceModule')
        .service('outboundService', ['$filter', '$http', outboundService]);

    function outboundService($filter, $http) {

        var createCampaignCommandUrl = '../api/Outbound/Campaign';
        var getCampaignCommandUrl = '../api/Outbound/Campaign/';
        var getCampaignLoadUrl = '../api/Outbound/Campaign/Load';
        var editCampaignCommandUrl = '../api/Outbound/Campaign/';
        var getCampaignStatisticsUrl = '../api/Outbound/Campaign/Statistics';
        var getFilteredCampaignsUrl = '../api/Outbound/Campaigns';
        var getCampaignVisualizationUrl = '../api/Outbound/Campaign/Visualization/';

        this.getCampaignVisualization = function (campaignId, successCb, errorCb) {
        	$http.post(getCampaignVisualizationUrl + campaignId) 
				.success(function (data) {
					if (successCb != null) successCb(normalizeChartData(data));
					console.log("raw data" , data);
		        })
				.error(function (data) {
					if (errorCb != null) errorCb(data);
				});
        };

		
		function normalizeChartData(data) {
			var data = angular.copy(data);
			var dates = [];
			var rawBacklogs = [];
			var calculatedBacklogs = [];
			var plans = [];
			var unscheduledPlans = [];
			var schedules= [];
			var underDiffs = [];
			var overDiffs = [];
			console.log('normalizing', data);

			if (!(data.Dates && data.BacklogPersonHours && data.ScheduledPersonHours && data.PlannedPersonHours)) {
				return;
			}

			data.Dates.forEach(function(e, i) { //BacklogPersonHours,Dates,PlannedPersonHours,ScheduledPersonHours
				dates[i] = new moment(e.Date).format("YYYY-MM-DD");

				rawBacklogs[i] = Math.round(data.BacklogPersonHours[i]);

				calculatedBacklogs[i] = (data.ScheduledPersonHours[i] > 0 && data.ScheduledPersonHours[i] < data.PlannedPersonHours[i]) ?
					Math.round(data.ScheduledPersonHours[i] + data.BacklogPersonHours[i] - data.PlannedPersonHours[i]) :
					Math.round(data.BacklogPersonHours[i]);

				plans[i] = Math.round(data.PlannedPersonHours[i]);

				unscheduledPlans[i] = (data.ScheduledPersonHours[i] > 0) ?
					0 :
					Math.round(data.PlannedPersonHours[i]);

				schedules[i] = Math.round(data.ScheduledPersonHours[i]);

				underDiffs[i] = (data.ScheduledPersonHours[i] > 0 && data.ScheduledPersonHours[i] < data.PlannedPersonHours[i]) ?
					Math.round(data.PlannedPersonHours[i] - data.ScheduledPersonHours[i]) :
					0;

				overDiffs[i] = (data.ScheduledPersonHours[i] > 0 && data.ScheduledPersonHours[i] > data.PlannedPersonHours[i]) ?
					Math.round(data.ScheduledPersonHours[i] - data.PlannedPersonHours[i]) :
					0;

			});

			return {
				dates: ['x'].concat(dates),
				rawBacklogs: ['backlog'].concat(rawBacklogs),
				calculatedBacklogs: ['backlog (calculated)'].concat(calculatedBacklogs),
				plans: ['planned'].concat(plans),
				unscheduledPlans: ['planned'].concat(unscheduledPlans),
				schedules: ['scheduled'].concat(schedules),
				underDiffs: ['under SLA'].concat(underDiffs),
				overDiffs: ['overstaffing'].concat(overDiffs)
			};
		}

		function selectDataGroups(viewScheduleDiffToggle, plannedPhase) {
			//return (viewScheduleDiffToggle) ? ['calculatedBacklogs', 'underDiffs', 'overDiffs', 'plans'] : ['rawBacklogs', 'schedules', 'unscheduledPlans'];
			if (viewScheduleDiffToggle) {
				if (plannedPhase==1) {
					return ['calculatedBacklogs', 'plans'];
				} else {
					return ['calculatedBacklogs', 'underDiffs', 'overDiffs', 'plans'];
				}
			} else {
				if (plannedPhase==1) {
					return ['rawBacklogs','unscheduledPlans'];
				} else {
					return ['rawBacklogs', 'schedules', 'unscheduledPlans'];
				}
				
			}
		}

		function getDataColor(name) {
			var colorMap = {
				rawBacklogs: '#1F77B4',
				calculatedBacklogs: '#1F77B4',
				plans: '#66C2FF',
				unscheduledPlans: '#66C2FF',
				schedules: '#C2E085',
				underDiffs: '#9467BD',
				overDiffs: '#f44336'
			};
			if (name)
				return colorMap[name];
			return colorMap;
		}

		function makeGraph(graph, graphId, viewScheduleDiffToggle, graphData, plannedPhase) {

			console.log('Graph data', graphData);
			var currentLabelGroups = selectDataGroups(viewScheduleDiffToggle, plannedPhase).map(function (name) { return graphData[name][0]; });
			var previousLabelGroups = selectDataGroups(!viewScheduleDiffToggle, plannedPhase).map(function (name) { return graphData[name][0]; });

			var colorMap = {};
			var dataColor = getDataColor();
			for (var name in dataColor) {
				colorMap[graphData[name][0]] = dataColor[name];
			}

			console.log(colorMap);

			if (graph) {
				console.log('reloading graph', currentLabelGroups);
				graph.load({
					columns: selectDataGroups(viewScheduleDiffToggle, plannedPhase).map(function (name) { return graphData[name]; }),
					unload:  previousLabelGroups
				});
			} else {
				//var tmp = [graphData.dates].concat(selectDataGroups(viewScheduleDiffToggle).map(function(name) { return graphData[name]; }));
				//console.log('making graph', tmp);

				graph = c3.generate({
					bindto: graphId ,
					data: {
						x: 'x',
						columns: [graphData.dates].concat(selectDataGroups(viewScheduleDiffToggle, plannedPhase).map(function (name) { return graphData[name]; })),
						type: 'bar',						
						groups: [
							currentLabelGroups,
							previousLabelGroups
						],
						colors: colorMap,
						order: 'null'
					},
					zoom: {
						enabled: true						
					},
					axis: {
						x: {
							type: 'timeseries',
							tick: {
								format: '%m-%d'								
							}
						},
						y: {							
							label: {
								text: 'Person hours (h)'								
							}
						}
					}
				});

			}
			return graph;
		}

		this.makeGraph = makeGraph;


		this.load = function(successCb) {
			$http.get(getCampaignLoadUrl).success(function(data) {
				if (successCb != null) successCb(data);
			});
		}

        this.getCampaignStatistics = function(filter, successCb, errorCb) {
            $http.get(getCampaignStatisticsUrl).success(function(data) {
                    if (successCb != null) successCb(data);
                }).
                error(function(data) {
                    if (errorCb != null) errorCb(data);
                });
        };

        this.listFilteredCampaigns = function(filter, successCb, errorCb) {
            $http.post(getFilteredCampaignsUrl, filter).
                success(function(data) {
                    if (successCb != null)
                        successCb(data);
                }).
                error(function(data) {
                    if (errorCb != null) errorCb(data);
                });
        };

        this.getCampaign = function (campaignId, successCb, errorCb) {
            $http.get(getCampaignCommandUrl + campaignId).
				success(function (data) {
				    if (successCb != null) successCb(denormalizeCampaign(data));
				}).
				error(function (data) {
				    if (errorCb != null) errorCb(data);
				});
        };

        this.addCampaign = function (campaign, successCb, errorCb) {
            $http.post(createCampaignCommandUrl, normalizeCampaign(campaign)).
				success(function (data) {
				    if (successCb != null) successCb(data);
				}).
				error(function (data) {
				    if (errorCb != null) errorCb(data);
				});
        };

        this.editCampaign = function (campaign, successCb, errorCb) {
            $http.put(editCampaignCommandUrl + campaign.Id, normalizeCampaign(campaign))
				.success(function (data) {
				    if (successCb != null) successCb(data);
				})
				.error(function (data) {
				    if (errorCb != null) errorCb(data);
				});
        };

        this.createEmptyWorkingPeriod = createEmptyWorkingPeriod;
        this.calculateCampaignPersonHour = calculateCampaignPersonHour;

        function calculateCampaignPersonHour(campaign) {
            var Target = campaign.CallListLen * campaign.TargetRate / 100,
				RPCR = campaign.RightPartyConnectRate / 100,
				CR = campaign.ConnectRate / 100,
				Unproductive = campaign.UnproductiveTime,
				ConnectAHT = campaign.ConnectAverageHandlingTime,
				RPCAHT = campaign.RightPartyAverageHandlingTime;

            if (RPCAHT == 0 || CR == 0) return null;

            var hours = (Target * (RPCAHT + Unproductive)
				+ (Target / RPCR - Target) * (ConnectAHT + Unproductive)
				+ (Target / (CR * RPCR) - Target / RPCR) * Unproductive) / 60 / 60;

            return hours;
        }

        function normalizeCampaign(campaign) {
            var campaign = angular.copy(campaign);

            var formattedWorkingHours = [];

            campaign.WorkingHours.forEach(function (d) {
                d.WeekDaySelections.forEach(function (e) {
                    if (e.Checked) {
                        formattedWorkingHours.push({
                            WeekDay: e.WeekDay,
                            StartTime: formatTimespanInput(d.StartTime),
                            EndTime: formatTimespanInput(d.EndTime)
                        });
                    }
                });
            });

            campaign.WorkingHours = formattedWorkingHours;
            return campaign;
        }

        function formatTimespanInput(dtObj) {
            return $filter('date')(dtObj, 'HH:mm');
        }

        function parseTimespanString(t) {
            if (!angular.isString(t)) return t;
            var parts = t.match(/^(\d{1,2}):(\d{1,2})(:|$)/);
            if (parts) {
                var d = new Date();
                d.setHours(parts[1]);
                d.setMinutes(parts[2]);
                return d;
            }

        }

        function denormalizeCampaign(campaign) {
            var campaign = angular.copy(campaign);

            if (campaign.StartDate) campaign.StartDate.Date = new Date(campaign.StartDate.Date);
            if (campaign.EndDate) campaign.EndDate.Date = new Date(campaign.EndDate.Date);

            var reformattedWorkingHours = [];


            campaign.WorkingHours.forEach(function (a) {

                var startTime = parseTimespanString(a.StartTime);
                var endTime = parseTimespanString(a.EndTime);

                var workingHourRows = reformattedWorkingHours.filter(function (wh) {
                    return formatTimespanInput(wh.StartTime) == formatTimespanInput(startTime)
						&& formatTimespanInput(wh.EndTime) == formatTimespanInput(endTime);
                });
                var workingHourRow;
                if (workingHourRows.length == 0) {
                    workingHourRow = createEmptyWorkingPeriod(startTime, endTime);

                    angular.forEach(workingHourRow.WeekDaySelections, function (e) {
                        if (e.WeekDay == a.WeekDay) e.Checked = true;
                    });
                    reformattedWorkingHours.push(workingHourRow);
                } else {
                    workingHourRow = workingHourRows[0];
                    angular.forEach(workingHourRow.WeekDaySelections, function (e) {
                        if (e.WeekDay == a.WeekDay) e.Checked = true;
                    });
                }

            });
            campaign.WorkingHours = reformattedWorkingHours;
            return campaign;
        };

        function createEmptyWorkingPeriod(startTime, endTime) {
            var weekdaySelections = [];
            var startDow = (moment.localeData()._week) ? moment.localeData()._week.dow : 0;

            for (var i = 0; i < 7; i++) {
                var curDow = (startDow + i) % 7;
                weekdaySelections.push({ WeekDay: curDow, Checked: false });
            }

            return { StartTime: startTime, EndTime: endTime, WeekDaySelections: weekdaySelections };
        }
    }


})();