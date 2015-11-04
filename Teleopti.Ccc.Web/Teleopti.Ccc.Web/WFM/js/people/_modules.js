(function() {
	angular.module('wfm.people', [
		'peopleService',
		'ui.grid.pagination',
		'ui.grid.resizeColumns',
		'ui.grid.importer',
		'ngAnimate',
		'ng-mfb',
		'ngMaterial',
		'ngFileUpload',
		'ui.indeterminate',
		'wfm.pagination'
	]);
})();