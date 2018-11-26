(function() {
	angular.module('wfm.intraday', [
		'gridshore.c3js.chart',
		'ngResource',
		'ui.router',
		'wfm.notice',
		'pascalprecht.translate',
		'wfm.autofocus',
		'toggleService',
		'angularMoment',
		'wfm.dateOffset',
		'wfm.utilities',
		'wfm.skillGroup',
		'wfm.skillPicker'
	]);
})();
