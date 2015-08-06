(function () {
    'use strict';

    angular.module('outboundServiceModule')
        .service('outboundChartService', ['$filter', '$http', 'outboundTranslationService', outboundChartService]);

    function outboundChartService($filter, $http, tl) {

        var getCampaignVisualizationUrl = '../api/Outbound/Campaign/Visualization/';      

        var translationKeys = [
           'Backlog',
           'Scheduled',
           'Planned',
           'Underscheduled',
           'Overscheduled',
           'Progress',
           'NeededPersonHours',
           'EndDate',
           'Today',
           'Start'
        ];
        var self = this;

        this.getCampaignVisualization = tl.applyTranslation(translationKeys, getCampaignVisualization, self);
        this.makeGraph = tl.applyTranslation(translationKeys, makeGraph, self);
        this.buildGraphDataSeqs = buildGraphDataSeqs;

        this.zip = zip;
        this.coreGetCampaignVisualization = getCampaignVisualization;
        this.coreMapGraphData = mapGraphData;

        function getCampaignVisualization(campaignId, successCb, errorCb) {         
            $http.get(getCampaignVisualizationUrl + campaignId).success(function (campaignData) {              
                if (successCb != null) successCb(self.buildGraphDataSeqs(campaignData));
            }).error(function(e) {
                if (errorCb != null) errorCb(e);
            });
        };      

        function mapGraphData(data) {            
            var returnData = {
                dates: null,
                rawBacklogs: null,
                calculatedBacklogs: null,
                plans: null,
                unscheduledPlans: null,
                schedules: null,
                underDiffs: null,
                overDiffs: null,
                progress: null
            };
          
            returnData.dates = new moment(data.Dates.Date).format("YYYY-MM-DD");
            returnData.rawBacklogs = data.BacklogPersonHours;
            returnData.calculatedBacklogs = (data.ScheduledPersonHours > 0 && data.ScheduledPersonHours < data.PlannedPersonHours)
                ? data.ScheduledPersonHours + data.BacklogPersonHours - data.PlannedPersonHours : data.BacklogPersonHours;					
            returnData.plans = data.PlannedPersonHours;
            returnData.unscheduledPlans = data.ScheduledPersonHours > 0 ? 0 : data.PlannedPersonHours;
            returnData.schedules = data.ScheduledPersonHours;
            returnData.underDiffs = data.ScheduledPersonHours > 0 && data.ScheduledPersonHours < data.PlannedPersonHours
                ? data.PlannedPersonHours - data.ScheduledPersonHours : 0;
            returnData.overDiffs = data.ScheduledPersonHours > 0 && data.ScheduledPersonHours > data.PlannedPersonHours
                ? data.ScheduledPersonHours - data.PlannedPersonHours : 0;
            returnData.progress = data.BacklogPersonHours;

            return returnData;
        }       

        function getDataLabels() {            
            return {               
                dates: 'x',
                rawBacklogs: self.dictionary['Backlog'], 
                calculatedBacklogs: self.dictionary['Backlog'] + ' ', 
                plans: self.dictionary['Planned'], 
                unscheduledPlans: self.dictionary['Planned'], 
                schedules: self.dictionary['Scheduled'],
                underDiffs: self.dictionary['Underscheduled'],
                overDiffs: self.dictionary['Overscheduled'],
                progress: self.dictionary['Progress'],
            };
        }

        function buildGraphDataSeqs(data) {
      
            var graphDataSeq = zip(data).map(function(d) {
                return mapGraphData(d);
            });

            if (!graphDataSeq || graphDataSeq <= 0) return;

            var beforeStartDate = new moment(graphDataSeq[0].dates).subtract(1, 'days').format("YYYY-MM-DD");

            var extrapolatedGraphData = {
                dates: beforeStartDate,
                rawBacklogs: 0,
                calculatedBacklogs: 0,
                plans: 0,
                unscheduledPlans: 0,
                schedules: 0,
                underDiffs: 0,
                overDiffs: 0,
                progress: graphDataSeq[0].rawBacklogs + graphDataSeq[0].unscheduledPlans + graphDataSeq[0].schedules
            };

            var labels = getDataLabels();

            var result = unzip([labels, extrapolatedGraphData].concat(graphDataSeq), function(v) {
                if (!isNaN(v) && isFinite(v))
                	return Math.round(v * 10) / 10;
                else
                    return v;
            });

            return result;
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
					return ['rawBacklogs', 'unscheduledPlans', 'progress'];
				} else {
					return ['rawBacklogs', 'schedules', 'unscheduledPlans', 'progress'];
				}
				
			}
		}

		function getDataColor(campaign) {
			var colorMap = {
				rawBacklogs: '#1F77B4',
				calculatedBacklogs: '#1F77B4',
				progress: '#2CA02C',
				plans: '#66C2FF',
				unscheduledPlans: '#66C2FF',
				schedules: '#26C6DA',
				underDiffs: '#9467BD',
				overDiffs: '#f44336'
			};

		    if (campaign) {
		        campaign.WarningInfo.forEach(function(e) {
		            if (e.TypeOfRule == 'OutboundUnderSLARule') {
		                colorMap.progress = '#F44336';
		            }
		            if (e.TypeOfRule == 'OutboundOverstaffRule') {
		                colorMap.progress = '#FF7F0E';
		            }
		        });
		    }

		    var dataColor = {};
		    var labels = getDataLabels();

			for (var name in colorMap) {
			    dataColor[labels[name]] = colorMap[name];
			}

			return dataColor;
		}

		function makeGraph(graph, campaign, viewScheduleDiffToggle, graphData, successCb) {
		    var graphId = '#Chart_' + campaign.Id,
		        plannedPhase = campaign.Status;		       
		  

			var currentLabelGroups = selectDataGroups(viewScheduleDiffToggle, plannedPhase).map(function (name) { return graphData[name][0]; });
			var previousLabelGroups = selectDataGroups(!viewScheduleDiffToggle, plannedPhase).map(function (name) { return graphData[name][0]; });

		    var dataColor = getDataColor(campaign);
		  
			if (graph) {
				graph.load({
					columns: selectDataGroups(viewScheduleDiffToggle, plannedPhase).map(function (name) { return graphData[name]; }),
					unload:  previousLabelGroups
				});
			} else {
                function _specifyAdditionalTypes() {
                    var obj = {};
                    obj[self.dictionary['Progress']] = 'line';
                    return obj;
                }

                function _specifyVerticalRange() {
                    return (Math.max.apply(Math, graphData.rawBacklogs.slice(1)) + Math.max.apply(Math, graphData.plans.slice(1))) * 1.1;
                }

                function _specifyDateHints() {
                    var endDate = new moment(campaign.EndDate.Date),
                        todayDate = new moment(),
                        startDate = new moment(campaign.StartDate.Date);

                    var hints = [
                        { value: endDate.format("YYYY-MM-DD"), text: self.dictionary['EndDate'] },
                        { value: startDate.format("YYYY-MM-DD"), text: self.dictionary['Start'] }
                    ];

                    if (todayDate >= startDate && todayDate <= endDate) {
                        hints.push({ value: todayDate.format("YYYY-MM-DD"), text: self.dictionary['Today'] });
                    }

                    return hints;
                }

                function _customizeTooltipfunction(d, defaultTitleFormat, defaultValueFormat, color) {
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

				graph = c3.generate({
					bindto: graphId,
					size: {
						height: 450
					},
					data: {
						x: 'x',
						columns: [graphData.dates].concat(selectDataGroups(viewScheduleDiffToggle, plannedPhase).map(function(name) { return graphData[name]; })),
						type: 'bar',
						types: _specifyAdditionalTypes(),
						groups: [
							currentLabelGroups,
							previousLabelGroups
						],
						colors: dataColor,
						order: 'null',
						onclick: function(d, ele) {
						}
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
							    text: self.dictionary['NeededPersonHours'],
                                position: 'outer-top'
							},
							max: _specifyVerticalRange(),
							min: 0,
						    padding: {bottom:0}
						}
					},
					grid: {
					     x: {
					         lines: _specifyDateHints()
					     }
					},
					tooltip: {
					    contents: _customizeTooltipfunction
					},
					subchart: {
					    show: true
					}
				});

			}

		    if (successCb) successCb(graph);		
		}

		

		function zip(data) {
		    var names = [],
                length = 0,
                result = [],
                i;

		    for (var name in data) {
		        if (!data.hasOwnProperty(name)) continue;
		        names.push(name);
		        length = data[name].length;
		    }

		    for (i = 0; i < length; i++) {
		        var obj = {};
		        angular.forEach(names, function (name) {
		            obj[name] = data[name][i];
		        });
		        result.push(obj);
		    }
		    return result;
		}

		function unzip(data, valueFilter) {
		    if (data.length > 0) {
		        var names = [],
                    returnData = {};
		        for (var name in data[0]) {
		            if (!data[0].hasOwnProperty(name)) continue;
		            names.push(name);
		            returnData[name] = [];
		        }

		        for (var i = 0; i < data.length; i++) {
		            angular.forEach(names, function (name) {
		                returnData[name].push(
                            valueFilter ? valueFilter(data[i][name]) : data[i][name]
                        );
		            });
		        }
		        return returnData;
		    }
		}

    }


})();