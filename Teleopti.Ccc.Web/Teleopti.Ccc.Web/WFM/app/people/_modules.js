(function() {
	'use strict';
	angular.module('wfm.people', [
		'toggleService',
		'peopleService',
		'ui.grid.pagination',
		'ui.grid.resizeColumns',
		'ui.grid.importer',
		'ngAnimate',
		'ngMaterial',
		'ngFileUpload',
		'ui.indeterminate',
		'wfm.pagination',
		'pascalprecht.translate',
		'ui.router',
		'wfm.multiplesearchinput'
	]);
})();
