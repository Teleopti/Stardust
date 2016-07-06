(function () {
    'use strict';
    angular.module('wfm.teamSchedule').filter('br', ['$sce', function ($sce) {
		return function (stringArray) {
			return $sce.trustAsHtml(stringArray.join('<br/>'));
		};
	}]);
})();
