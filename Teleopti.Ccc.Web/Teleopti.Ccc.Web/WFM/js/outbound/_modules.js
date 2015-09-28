(function() {
	'use strict';
	angular.module('wfm.outbound', [
			'outboundServiceModule',
			'ngAnimate',
			'pascalprecht.translate',
			'angularMoment',
			'wfm.cardList',
			'wfm.daterangepicker',
			'wfm.timerangepicker',
			'toggleService',
			'gantt',
			'gantt.table',
			'gantt.tooltips'
		]
	);
	angular.module('outboundServiceModule', ['ngResource', 'pascalprecht.translate']);
})();