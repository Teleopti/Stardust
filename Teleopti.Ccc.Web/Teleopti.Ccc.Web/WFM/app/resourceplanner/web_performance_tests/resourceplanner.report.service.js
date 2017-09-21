(function() {
	'use strict';
	angular.module('wfm.resourceplanner')
		.service('ResourcePlannerReportSrvc',
		[
			'$resource',
			function($resource) {
				function parseWeek(period) {
					var culturalDaysOff = {};
					culturalDaysOff.a = 6; //saturday
					culturalDaysOff.b = 0; //sunday
					culturalDaysOff.start = 1;
					period.forEach(function(node) {
						node.SkillDetails.forEach(function(subnode) {
							var day = new Date(subnode.Date).getDay();
							if (day === culturalDaysOff.a || day === culturalDaysOff.b) {
								return subnode.weekend = true;
							}
							if (day === culturalDaysOff.start) {
								return subnode.weekstart = true;
							}
						});
					});
				};
				var intraOptimize = $resource('../api/resourceplanner/planningperiod/:id/optimizeintraday', { id: '@id', runAsynchronously: function (d) { return d.runAsynchronously } }, {});

				function parseRelDif(period) {
					period.forEach(function(node) {
						node.SkillDetails.forEach(function(subnode) {
							if (isNaN(subnode.RelativeDifference)) {
								subnode.ColorId = 3;
							}
							var tempParseDif = (subnode.RelativeDifference * 100).toFixed(1);
							return subnode.parseDif = tempParseDif;
						});
					});
				};

				return {
					parseWeek: parseWeek,
					parseRelDif: parseRelDif,
					intraOptimize: intraOptimize.save
				}
			}
		]);
})();
