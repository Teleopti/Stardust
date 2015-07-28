(function () {
    'use strict';

    angular.module('outboundServiceModule')
        .service('outboundChartService', ['$filter', '$http', outboundChartService]);

    function outboundChartService($filter, $http) {

        var getCampaignVisualizationUrl = '../api/Outbound/Campaign/Visualization/';

        this.getCampaignVisualization = function (campaignId, successCb, errorCb) {
        	$http.post(getCampaignVisualizationUrl + campaignId) 
				.success(function (data) {
					if (successCb != null) successCb(normalizeChartData(data));
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
				underDiffs: ['underscheduled'].concat(underDiffs),
				overDiffs: ['overscheduled'].concat(overDiffs)
			};
		}

		function selectDataGroups(viewScheduleDiffToggle, plannedPhase) {
			if (viewScheduleDiffToggle) {
				if ($filter('showPhase')(plannedPhase) == 'Planned') {
					return ['calculatedBacklogs', 'plans'];
				} else {
					return ['calculatedBacklogs', 'underDiffs', 'overDiffs', 'plans'];
				}
			} else {
				if ($filter('showPhase')(plannedPhase) == 'Planned') {
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
			var currentLabelGroups = selectDataGroups(viewScheduleDiffToggle, plannedPhase).map(function (name) { return graphData[name][0]; });
			var previousLabelGroups = selectDataGroups(!viewScheduleDiffToggle, plannedPhase).map(function (name) { return graphData[name][0]; });

			var colorMap = {};
			var dataColor = getDataColor();
			for (var name in dataColor) {
				colorMap[graphData[name][0]] = dataColor[name];
			}

			if (graph) {
				graph.load({
					columns: selectDataGroups(viewScheduleDiffToggle, plannedPhase).map(function (name) { return graphData[name]; }),
					unload:  previousLabelGroups
				});
			} else {
				graph = c3.generate({
					bindto: graphId ,
					data: {
						x: 'x',
						columns: [graphData.dates].concat(selectDataGroups(viewScheduleDiffToggle, plannedPhase).map(function (name) { return graphData[name]; })),
						type: 'bar',
						types: {
							subtitudeBacklogs: 'line'
						},
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


    }


})();