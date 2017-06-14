(function() {
	'use strict';

	angular.module('outboundServiceModule')
		.service('outboundChartService', ['$filter', '$http', 'outboundTranslationService', outboundChartService]);

	function outboundChartService($filter, $http, outboundTranslationSvc) {
		var getCampaignVisualizationUrl = '../api/Outbound/Campaign/Visualization';
		var updateCampaignProductionPlanUrl = '../api/Outbound/Campaign/ManualPlan';
		var removeCampaignProductionPlanUrl = '../api/Outbound/Campaign/ManualPlan/Remove';
		var redoCampaignProductionPlanUrl = '../api/Outbound/Campaign/Replan';
		var updateCampaignBacklogUrl = '../api/Outbound/Campaign/ActualBacklog';
		var removeCampaignBacklogUrl = '../api/Outbound/Campaign/ActualBacklog/Remove';

		var translationKeys = [
			'Backlog',
			'Scheduled',
			'Planned',
			'Progress',
			'NeededPersonHours',
			'EndDate',
			'Today',
			'Start',
			'ManuallyPlanned',
			'ManualPlan',
			'ClosedDay',
			'Overstaffing',
			'AddBacklog',
			'IgnoredScheduleHint'
		];
		var self = this;

		self.updateManualPlan = updateManualPlan;
		self.removeManualPlan = removeManualPlan;
		self.replan = replan;
		self.updateBacklog = updateBacklog;
		self.removeActualBacklog = removeActualBacklog;
		self.getCampaignVisualization = outboundTranslationSvc.applyTranslation(translationKeys, getCampaignVisualization, self);
		self.buildGraphDataSeqs = buildGraphDataSeqs;

		self.zip = zip;
		self.coreGetCampaignVisualization = getCampaignVisualization;
		self.coreMapGraphData = mapGraphData;

		function getCampaignData(campaignData) {
			return {
				graphData: self.buildGraphDataSeqs(campaignData),
				rawManualPlan: campaignData.IsManualPlanned,
				manualBacklog: campaignData.IsActualBacklog,
				closedDays: campaignData.IsCloseDays,
				translations: self.dictionary
			}
		}

		function replan(input, successCb, errorCb) {
			var postData = {
				CampaignId: input.campaignId,
				SkipDates: input.ignoredDates
			};

			$http.post(redoCampaignProductionPlanUrl, postData).success(function(campaignData) {
				if (successCb != null) successCb(getCampaignData(campaignData));
			}).error(function(e) {
				if (errorCb != null) errorCb(e);
			});
		}

		function updateManualPlan(input, successCb, errorCb) {
			var postData = {
				CampaignId: input.campaignId,
				ManualProductionPlan: input.selectedDates.map(function(d) {
					return {
						Date: d,
						Time: input.manualPlanInput
					}
				}),
				SkipDates: input.ignoredDates
			};

			$http.post(updateCampaignProductionPlanUrl, postData).
			success(function(campaignData) {
				if (successCb != null) successCb(getCampaignData(campaignData));
			}).
			error(function(e) {
				if (errorCb != null) errorCb(e);
			});
		}

		function removeManualPlan(input, successCb, errorCb) {
			var postData = {
				CampaignId: input.campaignId,
				Dates: input.selectedDates,
				SkipDates: input.ignoredDates
			};

			$http.post(removeCampaignProductionPlanUrl, postData).
			success(function(campaignData) {
				if (successCb != null) successCb(getCampaignData(campaignData));
			}).
			error(function(e) {
				if (errorCb != null) errorCb(e);
			});
		}

		function getCampaignVisualization(input, successCb, errorCb) {
			$http.post(getCampaignVisualizationUrl, input).success(function (campaignData) {
				if (successCb != null) successCb(getCampaignData(campaignData));
			}).error(function(e) {
				if (errorCb != null) errorCb(e);
			});
		}

		function updateBacklog(input, successCb, errorCb) {
			$http.post(updateCampaignBacklogUrl, {
				CampaignId: input.campaignId,
				ActualBacklog: input.selectedDates.map(function(d) {
					return {
						Date: d,
						Time: input.manualBacklogInput
					};
				}),
				SkipDates: input.ignoredDates
			}).
			success(function(campaignData) {
				if (successCb != null) successCb(getCampaignData(campaignData));
			}).
			error(function(e) {
				if (errorCb != null) errorCb(e);
			});
		}

		function removeActualBacklog(input, successCb, errorCb) {
			$http.post(removeCampaignBacklogUrl, {
				CampaignId: input.campaignId,
				Dates: input.selectedDates,
				SkipDates: input.ignoredDates
			}).
			success(function(campaignData) {
				if (successCb != null) successCb(getCampaignData(campaignData));
			}).
			error(function(e) {
				if (errorCb != null) errorCb(e);
			});
		}

		function mapGraphData(data) {
			var returnData = {
				dates: null,
				rawBacklogs: null,
				rawPlans: null,
				rawSchedules: null,
				unscheduledPlans: null,
				schedules: null,
				progress: null,
				overStaff: null
			};

			returnData.dates = moment(data.Dates).format("YYYY-MM-DD");
			returnData.rawBacklogs = data.BacklogPersonHours;
			returnData.rawPlans = data.PlannedPersonHours;
			returnData.rawSchedules = data.ScheduledPersonHours;
			returnData.unscheduledPlans = data.PlannedPersonHours;
			returnData.schedules = data.ScheduledPersonHours;
			returnData.progress = data.BacklogPersonHours;
			returnData.overStaff = data.OverstaffPersonHours;

			if (returnData.schedules > 0) {
				returnData.unscheduledPlans = 0;
				returnData.schedules -= returnData.overStaff;
			} else {
				returnData.unscheduledPlans -= returnData.overStaff;
			}

			return returnData;
		}

		function getDataLabels() {
			return {
				dates: 'x',
				rawBacklogs: self.dictionary['Backlog'],
				rawPlans: self.dictionary['xRawPlans'],
				rawSchedules: self.dictionary['xRawSchedules'],
				unscheduledPlans: self.dictionary['Planned'],
				schedules: self.dictionary['Scheduled'],
				progress: self.dictionary['Progress'],
				overStaff: self.dictionary['Overstaffing']
			};
		}

		function buildGraphDataSeqs(data) {
			var graphDataSeq = zip(data).map(function(d) {
				return mapGraphData(d);
			});

			if (!graphDataSeq || graphDataSeq <= 0) return;

			var beforeStartDate = moment(graphDataSeq[0].dates).subtract(1, 'days').format("YYYY-MM-DD");

			var extrapolatedGraphData = {
				dates: beforeStartDate,
				rawBacklogs: 0,
				rawPlans: 0,
				rawSchedules: 0,
				unscheduledPlans: 0,
				schedules: 0,
				progress: graphDataSeq[0].rawBacklogs + graphDataSeq[0].unscheduledPlans + graphDataSeq[0].schedules,
				overStaff: 0
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
				angular.forEach(names, function(name) {
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
					angular.forEach(names, function(name) {
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