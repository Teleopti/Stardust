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
			'gantt.table',
			'gantt.tooltips'
		]
	);
	angular.module('outboundServiceModule', ['ngResource', 'pascalprecht.translate']);
})();