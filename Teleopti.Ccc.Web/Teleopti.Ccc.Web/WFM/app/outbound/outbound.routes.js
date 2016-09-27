(function () {
	'use strict';

	angular
	.module('wfm.outbound')
	.config(stateConfig);

	function stateConfig($stateProvider) {
		$stateProvider.state('outbound', {
			url: '/outbound',
			templateUrl: 'app/outbound/html/outbound.html',
			controller: 'OutboundDefaultCtrl'
		}).state('outbound.summary', {
			url: '/summary',
			views: {
				'': {
					templateUrl: 'app/outbound/html/outbound-overview.html'
				},
				'gantt@outbound.summary': {
					templateUrl: 'app/outbound/html/campaign-list-gantt.html',
					controller: 'CampaignListGanttCtrl'
				}
			}
		}).state('outbound.create', {
			url: '/create',
			templateUrl: 'app/outbound/html/campaign-create.html',
			controller: 'OutboundCreateCtrl'
		}).state('outbound.edit', {
			url: '/campaign/:Id',
			templateUrl: 'app/outbound/html/campaign-edit.html',
			controller: 'OutboundEditCtrl'
		})
	}
})();
