(function() {
	angular.module('wfm.people', [
		'peopleService',
		'ui.grid.pagination',
		'ui.grid.resizeColumns',
		'ui.grid.importer',
		'ngAnimate',
		'ngMaterial',
		'ngFileUpload',
		'ui.indeterminate',
		'wfm.pagination'
	]);
})();