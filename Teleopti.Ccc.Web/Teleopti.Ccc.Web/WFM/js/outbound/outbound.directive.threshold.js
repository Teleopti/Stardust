(function() {
	'use strict';

	angular.module('wfm.outbound').directive('threshold', ['outboundChartService', thresholdCtrl]);

	function thresholdCtrl(outboundChartService) {
		return {
			link: postLink
		};

		function postLink(scope, elm, attrs, scrollfixTarget) {
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
			scope.$on('outbound.updateThreshold', function(_s, threshold) {
				for (var i = scope.lastManualBacklogDay; i < scope.campaignData.rawBacklogs.length; i++) {
					var overStaff = scope.campaignData.overStaff[i];
					var backlog = scope.campaignData.rawBacklogs[i];
					var plan = scope.campaignData.unscheduledPlans[i];
					overStaffed[i] = (overStaff - backlog) > backlog * threshold / 100;
					underSLA [i]= (backlog * 2 - plan) > backlog * threshold / 100;
				}
				var isOverStaffed = overStaffed.indexOf(true);
				var isUnderSLA = underSLA.indexOf(true);
				if (!elm.hasClass('threshold-overStaffed') && isOverStaffed) {
					elm.addClass('threshold-overStaffed');
				} else if (elm.hasClass('threshold-overStaffed') && !isOverStaffed) {
					elm.removeClass('threshold-overStaffed');
				}
				if (!elm.hasClass('threshold-underSLA') && isUnderSLA) {
					elm.addClass('threshold-underSLA');
				} else if (elm.hasClass('threshold-underSLA') && !isUnderSLA) {
					elm.removeClass('threshold-underSLA');
				}
			});

		}
	}
})();