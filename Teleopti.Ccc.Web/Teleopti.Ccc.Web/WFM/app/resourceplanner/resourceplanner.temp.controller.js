(function() {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('ResourceplannerTempCtrl', [
			'$scope', '$state', '$stateParams','ResourcePlannerReportSrvc',
			function($scope, $state, $stateParams, ResourcePlannerReportSrvc) {
				$scope.isEnabled = false;
				var status = "waiting";
				$scope.optimizeRunning = false;
				$scope.checkStatus = function() {
					return status;
				}
				$scope.intraOptimize = function() {
					status = 'running';
					ResourcePlannerReportSrvc.intraOptimize({
						id: $stateParams.id,
						runAsynchronously: false
					}).$promise.then(function(result) {
						status = 'done';
					},function(reason){
						status = 'error';
					}
				);
				};
			}
		]);
})();
