﻿(function () {
    'use strict';

    angular.module('outboundServiceModule')
        .service('outboundChartService', ['$filter', '$http', 'outboundTranslationService', outboundChartService]);

    function outboundChartService($filter, $http, tl) {

    	var getCampaignVisualizationUrl = '../api/Outbound/Campaign/Visualization/';
    	var updateCampaignProductionPlanUrl = '../api/Outbound/Campaign/ManualPlan';
    	var removeCampaignProductionPlanUrl = '../api/Outbound/Campaign/ManualPlan/Remove';
    	var redoCampaignProductionPlanUrl = '../api/Outbound/Campaign/Replan/';
    	var updateCampaignBacklogUrl = '../api/Outbound/Campaign/ActualBacklog';
	    var removeCampaignBacklogUrl = '../api/Outbound/Campaign/ActualBacklog/Remove';

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
           'Start',
		   'ManuallyPlanned',
		   'Closed',
		   'Overstaff'
        ];
        var self = this;

        this.updateManualPlan = updateManualPlan;
        this.updateBacklog = updateBacklog;

        this.replan = function (campaignId, successCb, errorCb) {
        	$http.get(redoCampaignProductionPlanUrl + campaignId).success(function () {
		        if (successCb != null) successCb();
	        }).error(function (e) {
				if (errorCb != null) errorCb(e);
			});
		}

        this.removeManualPlan = function (manualProductionPlan, successCb, errorCb) {
        	$http.post(removeCampaignProductionPlanUrl, manualProductionPlan).
                success(function (campaignData) {
                	if (successCb != null) successCb(self.buildGraphDataSeqs(campaignData), campaignData.IsManualPlanned);
                }).
                error(function (e) {
                	if (errorCb != null) errorCb(e);
                });
        }

        this.removeActualBacklog = function (actualBacklog, successCb, errorCb) {
        	$http.post(removeCampaignBacklogUrl, actualBacklog).
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
        		if (successCb != null) successCb(self.buildGraphDataSeqs(campaignData), self.dictionary, campaignData.IsManualPlanned, campaignData.IsCloseDays);
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

        function updateBacklog(backlog, successCb, errorCb) {
        	$http.post(updateCampaignBacklogUrl, backlog).
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
                unscheduledPlans: null,
                schedules: null,
                progress: null,
                overStaff: null
        };
          
            returnData.dates = new moment(data.Dates.Date).format("YYYY-MM-DD");
            returnData.rawBacklogs = data.BacklogPersonHours;
            returnData.unscheduledPlans = data.ScheduledPersonHours > 0 ? 0 : data.PlannedPersonHours;
            returnData.schedules = data.ScheduledPersonHours;
            returnData.progress = data.BacklogPersonHours;
            returnData.overStaff = data.OverstaffPersonHours;

            return returnData;
        }       

        function getDataLabels() {
            return {               
                dates: 'x',
                rawBacklogs: self.dictionary['Backlog'],
                unscheduledPlans: self.dictionary['Planned'], 
                schedules: self.dictionary['Scheduled'],
                progress: self.dictionary['Progress'],
				overStaff: self.dictionary['Overstaff']
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
                unscheduledPlans: 0,
                schedules: 0,
                progress: graphDataSeq[0].rawBacklogs + graphDataSeq[0].unscheduledPlans + graphDataSeq[0].schedules,
				overStaff:0
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