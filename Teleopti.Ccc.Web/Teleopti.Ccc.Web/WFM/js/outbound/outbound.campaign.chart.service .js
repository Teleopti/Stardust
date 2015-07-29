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

			data.Dates.forEach(function(e, i) {
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
				rawBacklogs: ['Backlog'].concat(rawBacklogs),
				calculatedBacklogs: ['Backlog '].concat(calculatedBacklogs),
				plans: ['Planned'].concat(plans),
				unscheduledPlans: ['Planned'].concat(unscheduledPlans),
				schedules: ['Scheduled'].concat(schedules),
				underDiffs: ['Underscheduled'].concat(underDiffs),
				overDiffs: ['Overscheduled'].concat(overDiffs),
				statusLine: ['Progress Line'].concat(rawBacklogs)
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
					return ['rawBacklogs', 'unscheduledPlans', 'statusLine'];
				} else {
					return ['rawBacklogs', 'schedules', 'unscheduledPlans', 'statusLine'];
				}
				
			}
		}

		function getDataColor(name) {
			var colorMap = {
				rawBacklogs: '#1F77B4',
				calculatedBacklogs: '#1F77B4',
				statusLine: '#2CA02C',
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

		function makeGraph(graph, campaign, viewScheduleDiffToggle, graphData) {
		    var graphId = '#Chart_' + campaign.Id,
		        plannedPhase = campaign.Status,
		        warningInfo = campaign.WarningInfo,
		        endDate = new moment(campaign.EndDate.Date).format("YYYY-MM-DD");
		
			var currentLabelGroups = selectDataGroups(viewScheduleDiffToggle, plannedPhase).map(function (name) { return graphData[name][0]; });
			var previousLabelGroups = selectDataGroups(!viewScheduleDiffToggle, plannedPhase).map(function (name) { return graphData[name][0]; });
			
			var dataColor = getDataColor();
			warningInfo.forEach(function (e) {
			    if (e.TypeOfRule == 'OutboundUnderSLARule') {			       
			        dataColor.statusLine = '#9467BD';
			    }
			    if (e.TypeOfRule == 'OutboundOverstaffRule') {			       
			        dataColor.statusLine = '#f44336';
			    }
			});

			var colorMap = {};
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
						    'Progress Line': 'line'
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
					},
					grid: {
					     x: {
					         lines: [{ value: endDate, text: 'End Date' }]
					     }
					},
					tooltip: {
					    contents: function (d, defaultTitleFormat, defaultValueFormat, color) {
					        var $$ = this, config = $$.config,
                                titleFormat = config.tooltip_format_title || defaultTitleFormat,
                                nameFormat = config.tooltip_format_name || function (name) { return name; },
                                valueFormat = config.tooltip_format_value || defaultValueFormat,
                                text, i, title, value, name, bgcolor;
					        for (i = d.length - 1 ; i >= 0; i--) {
					            if (!(d[i] && (d[i].value))) { continue; }

					            if (!text) {
					                title = titleFormat ? titleFormat(d[i].x) : d[i].x;
					                text = "<table class='" + $$.CLASS.tooltip + "'>" + (title || title === 0 ? "<tr><th colspan='2'>" + title + "</th></tr>" : "");
					            }

					            name = nameFormat(d[i].name);
					            value = valueFormat(d[i].value, d[i].ratio, d[i].id, d[i].index);
					            bgcolor = $$.levelColor ? $$.levelColor(d[i].value) : color(d[i].id);

					            text += "<tr class='" + $$.CLASS.tooltipName + "-" + d[i].id + "'>";
					            text += "<td class='name' style='text-align: left' ><span style='background-color:" + bgcolor + "'></span>" + name + "</td>";
					            text += "<td class='value'>" + value + "</td>";
					            text += "</tr>";
					        }
					        if (text)
					            return text + "</table>";
					        else return '';
					    }
					},
					subchart: {
					    show: true
					}
				});

			}
			return graph;
		}

		this.makeGraph = makeGraph;


    }


})();