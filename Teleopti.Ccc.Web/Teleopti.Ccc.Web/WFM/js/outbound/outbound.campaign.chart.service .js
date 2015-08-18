﻿(function () {
    'use strict';

    angular.module('outboundServiceModule')
        .service('outboundChartService', ['$filter', '$http', 'outboundTranslationService', outboundChartService]);

    function outboundChartService($filter, $http, tl) {

    	var getCampaignVisualizationUrl = '../api/Outbound/Campaign/Visualization/';
    	var updateCampaignProductionPlanUrl = '../api/Outbound/Campaign/ManualPlan';
    	var removeCampaignProductionPlanUrl = '../api/Outbound/Campaign/ManualPlan/Remove';

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

        this.updateManualPlan = updateManualPlan;


        this.removeManualPlan = function (manualProductionPlan, successCb, errorCb) {
        	$http.post(removeCampaignProductionPlanUrl, manualProductionPlan).
                success(function (campaignData) {
                	if (successCb != null) successCb(self.buildGraphDataSeqs(campaignData), campaignData.IsManualPlanned);
                }).
                error(function (e) {
                	if (errorCb != null) errorCb(e);
                });
        }

        this.getCampaignVisualization = tl.applyTranslation(translationKeys, getCampaignVisualization, self);      
        this.buildGraphDataSeqs = buildGraphDataSeqs;

        this.zip = zip;
        this.coreGetCampaignVisualization = getCampaignVisualization;
        this.coreMapGraphData = mapGraphData;

        function getCampaignVisualization(campaignId, successCb, errorCb) {         
        	$http.get(getCampaignVisualizationUrl + campaignId).success(function (campaignData) {
        		if (successCb != null) successCb(self.buildGraphDataSeqs(campaignData), self.dictionary, campaignData.IsManualPlanned);
            }).error(function(e) {
                if (errorCb != null) errorCb(e);
            });
        }

        function updateManualPlan(manualProductionPlan, successCb, errorCb) {
        	$http.post(updateCampaignProductionPlanUrl, manualProductionPlan).
                success(function (campaignData) {
                	if (successCb != null) successCb(self.buildGraphDataSeqs(campaignData), campaignData.IsManualPlanned);
                }).
                error(function (e) {
                	if (errorCb != null) errorCb(e);
                });
        }

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
            returnData.calculatedBacklogs = (data.BacklogPersonHours > 0)? 
                ((data.ScheduledPersonHours > 0 && data.ScheduledPersonHours < data.PlannedPersonHours)
                ? data.ScheduledPersonHours + data.BacklogPersonHours - data.PlannedPersonHours : data.BacklogPersonHours)
                : 0;					
            returnData.plans = data.PlannedPersonHours;
            returnData.unscheduledPlans = data.ScheduledPersonHours > 0 ? 0 : data.PlannedPersonHours;
            returnData.schedules = data.ScheduledPersonHours;
            returnData.underDiffs = data.ScheduledPersonHours > 0 && data.ScheduledPersonHours < data.PlannedPersonHours ?
                data.PlannedPersonHours - data.ScheduledPersonHours : 0;
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