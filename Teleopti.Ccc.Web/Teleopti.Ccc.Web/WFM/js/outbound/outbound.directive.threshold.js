(function() {
	'use strict';

	angular.module('wfm.outbound').directive('threshold', ['outboundChartService', thresholdCtrl]);

	function thresholdCtrl(outboundChartService) {
		return {
			link: postLink
		};

		function postLink(scope, elm) {
			if (scope.row.model.id.indexOf("_") < 0) {
				outboundChartService.getCampaignVisualization(scope.row.model.id, function(data, translations, manualPlan, closedDays, backlog) {
					scope.campaignData = data;
					scope.isManualBacklog = backlog;
					scope.campaignData.rawBacklogs.splice(0, 2);
					scope.campaignData.overStaff.splice(0, 2);
					scope.campaignData.unscheduledPlans.splice(0, 2);
					scope.lastManualBacklogDay = scope.isManualBacklog.lastIndexOf(true);
				});
				
				var overStaffed = [];
				var underSLA = [];
				scope.$on('outbound.updateThreshold', function (_s, threshold) {
					for (var i = scope.lastManualBacklogDay; i < scope.campaignData.rawBacklogs.length; i++) {
						var overStaff = scope.campaignData.overStaff[i];
						var backlog = scope.campaignData.rawBacklogs[i];
						var plan = scope.campaignData.unscheduledPlans[i];
						overStaffed[i] = (overStaff - backlog) > backlog * threshold / 100;
						underSLA[i] = (backlog * 2 - plan) > backlog * threshold / 100;
					}
					var isOverStaffed = overStaffed.indexOf(true);
					var isUnderSLA = underSLA.indexOf(true);
					if (!elm.hasClass('campaign-early') && isOverStaffed > 0) {
						elm.addClass('campaign-early');
					} else if (elm.hasClass('campaign-early') && isOverStaffed < 0) {
						elm.removeClass('campaign-early');
					}
					if (!elm.hasClass('campaign-late') && isUnderSLA > 0) {
						elm.addClass('campaign-late');
					} else if (elm.hasClass('campaign-late') && isUnderSLA < 0) {
						elm.removeClass('campaign-late');
					}
				});
			}

		}
	}
})();