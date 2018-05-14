(function () {
    angular.module('wfm.staffing', [
      'ui.router',
      'pascalprecht.translate',
      'gantt',
			'gantt.table',
		'gantt.tooltips',
		'gantt.table',
      'wfm.notice',
      'toggleService',
      'wfm.utilities',
      'wfm.skillGroup'
    ]);
})();
