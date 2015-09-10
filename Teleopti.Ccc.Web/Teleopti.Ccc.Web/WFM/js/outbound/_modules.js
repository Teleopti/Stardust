(function() {
	'use strict';
	angular.module('wfm.outbound', [
			'outboundServiceModule',
			'ngAnimate',
			'pascalprecht.translate',
			'wfm.cardList',
			'wfm.daterangepicker',
			'toggleService',
			'gantt',
			'gantt.table'
			
		]
	).constant('toggleKeyGantt', 'Wfm_Outbound_Campaign_GanttChart_34259');
	angular.module('outboundServiceModule', ['ngResource', 'pascalprecht.translate']);
})();