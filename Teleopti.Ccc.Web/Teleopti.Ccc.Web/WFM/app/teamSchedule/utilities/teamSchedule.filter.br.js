(function() {
	'use strict';
	angular.module('wfm.teamSchedule').filter('br', ['$sce', brFilter]);

	function brFilter($sce) {
		return function(stringArray) {
			return $sce.trustAsHtml(stringArray.join('<br/>'));
		};
	}
})();