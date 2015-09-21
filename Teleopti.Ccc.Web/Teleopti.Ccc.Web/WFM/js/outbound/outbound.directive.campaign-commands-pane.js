(function() {

	'use strict';

	angular.module('wfm.outbound')
		.directive('campaignCommandsPane', campaignCommandsPane);

	function campaignCommandsPane() {

		return {
			restrict: 'E',
			templateUrl: 'html/outbound/campaign-commands-pane.tpl.html',
			scope: {
				campaign: '=',
				selectedDates: '=',
				selectedDatesClosed: '=',
				isLoading: '=',
				callbacks: '='
			},
			controller: ['$scope', '$state', 'outboundService', 'outboundChartService', campaignCommandsPaneCtrl],
			link: postlink
		};

		function campaignCommandsPaneCtrl($scope, $state, outboundService, outboundChartService) {
			$scope.manualPlanSwitch = false;
			$scope.manualBacklogSwitch = false;
			$scope.manualPlanInput = null;
			$scope.manualBacklogInput = null;

			$scope.addManualPlan = addManualPlan;
			$scope.removeManualPlan = removeManualPlan;
			$scope.addManualBacklog = addManualBacklog;
			$scope.removeManualBacklog = removeManualBacklog;
			$scope.replan = replan;
			$scope.validManualProductionPlan = validManualProductionPlan;
			$scope.validManualBacklog = validManualBacklog;
			$scope.showDateSelectionHint = showDateSelectionHint;

			$scope.gotoEditCampaign = function () {
				if ($scope.campaign)
					$state.go('outbound.edit', { Id: $scope.campaign.Id });
			};

			$scope.toggleManualPlan = function() {
				$scope.manualPlanSwitch = !$scope.manualPlanSwitch;
				$scope.manualBacklogSwitch = false;
			}

			$scope.toggleManualBacklog = function() {
				$scope.manualBacklogSwitch = !$scope.manualBacklogSwitch;
				$scope.manualPlanSwitch = false;
			}
		
			function unclosedDays() {
				return $scope.selectedDates.filter(function (d) {
					return $scope.selectedDatesClosed.indexOf(d) < 0;
				});
			}

			function callbackDone() {
				$scope.manualPlanInput = null;
				$scope.manualBacklogInput = null;				
				$scope.isLoading = true;
			}

			function addManualPlan() {
				$scope.isLoading = true;
				outboundChartService.updateManualPlan({
					campaignId: $scope.campaign.Id,
					selectedDates: unclosedDays(),
					manualPlanInput: $scope.manualPlanInput
				}, function (response) {				
					if (angular.isDefined($scope.callbacks.addManualPlan)) {
						$scope.callbacks.addManualPlan(response, callbackDone);
					} else {
						callbackDone();
					}					
				});									
			}

			function removeManualPlan() {
				$scope.isLoading = true;
				outboundChartService.removeManualPlan({
					campaignId: $scope.campaign.Id,
					selectedDates: $scope.selectedDates
				}, function (response) {				
					if (angular.isDefined($scope.callbacks.removeManualPlan)) {
						$scope.callbacks.removeManualPlan(response, callbackDone);
					} else {
						callbackDone();
					}
				});				
			}

			function addManualBacklog() {
				$scope.isLoading = true;
				outboundChartService.updateBacklog({
					campaignId: $scope.campaign.Id,
					selectedDates: $scope.selectedDates,
					manualBacklogInput: $scope.manualBacklogInput
				}, function (response) {
					if (angular.isDefined($scope.callbacks.addManualBacklog)) {
						$scope.callbacks.addManualBacklog(response, callbackDone);
					} else {
						callbackDone();
					}
				});
			}

			function removeManualBacklog() {
				$scope.isLoading = true;
				outboundChartService.removeActualBacklog({
					campaignId: $scope.campaign.Id,
					selectedDates: $scope.selectedDates
				}, function (response) {
					if (angular.isDefined($scope.callbacks.removeManualBacklog)) {
						$scope.callbacks.removeManualBacklog(response, callbackDone);
					} else {
						callbackDone();
					}
				});
			}

			function replan() {
				$scope.isLoading = true;
				outboundChartService.replan($scope.campaign.Id, function (response) {
					if (angular.isDefined($scope.callbacks.replan)) {
						$scope.callbacks.replan(response, callbackDone);
					} else {
						callbackDone();
					}
				});
			}


			function validManualProductionPlan() {			
				return $scope.manualPlanInput && $scope.manualPlanInput >= 0 && $scope.selectedDates && $scope.selectedDates.length > 0;
			}

			function validManualBacklog() {				
				return $scope.manualBacklogInput && $scope.manualBacklogInput >= 0 && $scope.selectedDates && $scope.selectedDates.length > 0;
			}

			function showDateSelectionHint() {
				if ($scope.isLoading) return false;
				if ($scope.selectedDates && $scope.selectedDates.length > 0) return false;
				if ($scope.manualPlanSwitch) return  $scope.manualPlanInput != null;
				if ($scope.manualBacklogSwitch) return  $scope.manualBacklogInput != null;
				return false;
			}

		}

		function postlink(scope, elem, attrs) {
			
		


		}

		
	}

})();