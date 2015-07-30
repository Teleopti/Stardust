(function () {
    'use strict';

    angular.module('outboundServiceModule')
        .service('outboundChartService', ['$filter', '$http', '$translate', '$q', outboundChartService]);

    function outboundChartService($filter, $http, $translate, $q) {

        var getCampaignVisualizationUrl = '../api/Outbound/Campaign/Visualization/';

        var translationKeys = ['Backlog', 'Scheduled', 'Planned', 'Underscheduled', 'Overscheduled', 'Progress', 'NeededPersonHours', 'EndDate', 'Today', 'Start'];
        var translations = translationKeys.map(function (x) { return $translate(x); });
        var translationDictionary = {};


        this.getCampaignVisualization = function (campaignId, successCb, errorCb) {

            var tasks = [$http.post(getCampaignVisualizationUrl + campaignId)].concat(translations);

            $q.all(tasks).then(function (results) {               
                var campaignData = results.shift().data;
                for (var i = 0; i < translationKeys.length; i++) {
                    translationDictionary[translationKeys[i]] = results[i];
                }
                if (successCb != null) successCb(normalizeChartData(campaignData));
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

		    var beforeStartDate,
		        initialBacklog;

			if (!(data.Dates && data.BacklogPersonHours && data.ScheduledPersonHours && data.PlannedPersonHours)) {
				return;
			}

			data.Dates.forEach(function(e, i) {
			    dates[i] = new moment(e.Date).format("YYYY-MM-DD");

			    if (!beforeStartDate) beforeStartDate = new moment(e.Date).subtract(1, 'days').format("YYYY-MM-DD");

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

                if (!initialBacklog) {
                    initialBacklog = rawBacklogs[i] + plans[i] + schedules[i];
                }

			});

			return {
			    dates: ['x', beforeStartDate].concat(dates),
				rawBacklogs: [translationDictionary['Backlog'], 0].concat(rawBacklogs),
				calculatedBacklogs: [translationDictionary['Backlog'] + ' ', 0].concat(calculatedBacklogs),
				plans: [translationDictionary['Planned', 0]].concat(plans),
				unscheduledPlans: [translationDictionary['Planned'], 0].concat(unscheduledPlans),
				schedules: [translationDictionary['Scheduled'], 0].concat(schedules),
				underDiffs: [translationDictionary['Underscheduled'], 0].concat(underDiffs),
				overDiffs: [translationDictionary['Overscheduled'], 0].concat(overDiffs),
				statusLine: [translationDictionary['Progress'], initialBacklog].concat(rawBacklogs)
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
		        endDate = new moment(campaign.EndDate.Date).format("YYYY-MM-DD"),
		        todayDate = new moment().format("YYYY-MM-DD"),
		        startDate = new moment(campaign.StartDate.Date).format("YYYY-MM-DD");

		    var maxPersonHours = (Math.max.apply(Math, graphData.rawBacklogs.slice(1)) + Math.max.apply(Math, graphData.plans.slice(1))) * 1.5;
		  
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
                function _specifyAdditionalTypes() {
                    var obj = {};
                    obj[translationDictionary['Progress']] = 'line';
                    return obj;
                }

				graph = c3.generate({
				    bindto: graphId,                   
					data: {
						x: 'x',
						columns: [graphData.dates].concat(selectDataGroups(viewScheduleDiffToggle, plannedPhase).map(function (name) { return graphData[name]; })),
						type: 'bar',
						types: _specifyAdditionalTypes(),
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
							    text: translationDictionary['NeededPersonHours']
							},
							max: maxPersonHours,
							min: 0,
						    padding: {bottom:0}
						}
					},
					grid: {
					     x: {
					         lines: [
                                 { value: endDate, text: translationDictionary['EndDate'] },
                                 { value: todayDate, text: translationDictionary['Today'] },
                                 { value: startDate, text: translationDictionary['Start']}
					         ]
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

				graph.load({
				    columns: selectDataGroups(viewScheduleDiffToggle, plannedPhase).map(function (name) { return graphData[name]; }),
				    unload: currentLabelGroups
				});

			}
			return graph;
		}

		this.makeGraph = makeGraph;


    }


})();