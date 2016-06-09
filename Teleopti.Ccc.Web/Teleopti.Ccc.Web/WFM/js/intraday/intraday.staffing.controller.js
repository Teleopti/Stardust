(function () {
	'use strict';
	angular.module('wfm.intraday')
		.controller('IntradayStaffingCtrl', [
			'$scope', '$state','$stateParams', 'intradayService', '$filter', 'NoticeService', '$translate',
			function ($scope, $state, $stateParams, intradayService, $filter, NoticeService, $translate) {
				$scope.intervalDate = $stateParams.intervalDate;


			}
		]);
})();
