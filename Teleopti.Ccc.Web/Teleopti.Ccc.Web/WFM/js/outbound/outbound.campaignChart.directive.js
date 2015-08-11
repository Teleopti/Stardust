(function () {
	'use strict';

	angular.module('wfm.outbound').directive('campaignChart', [campaignChart]);

	function campaignChart() {
		return {
			controller: ['$scope', '$element', 'outboundChartService', campaignChartCtrl],
			template: '<div id="Chart_{{campaign.Id}}"></div>',
			scope: {
				'campaign': '=?'
			}
		};

		function campaignChartCtrl($scope, $element, outboundChartService) {
			var campaign = $scope.campaign;
			(function generateChart(campaign) {
				if (campaign.chart) return;

				outboundChartService.getCampaignVisualization(campaign.Id, function success(data) {
					campaign.viewScheduleDiffToggle = false;
					campaign.chartData = data;
					outboundChartService.makeGraph(null, campaign, campaign.viewScheduleDiffToggle, data, function (graph) {
						campaign.chart = graph;
					});
				});
			})(campaign);
		}

	};





})();