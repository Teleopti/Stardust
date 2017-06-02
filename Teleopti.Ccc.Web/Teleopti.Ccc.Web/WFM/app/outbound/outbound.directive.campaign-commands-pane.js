(function() {

	'use strict';

	angular.module('wfm.outbound')
		.directive('campaignCommandsPane', campaignCommandsPane);

	function campaignCommandsPane() {

		return {
			restrict: 'E',
			templateUrl: 'app/outbound/html/campaign-commands-pane.tpl.html',
			scope: {
				campaign: '=',
				selectedDates: '=',
				selectedDatesClosed: '=',
				isLoading: '=',
				callbacks: '='
			},
			controller: ['$scope', '$state', 'outboundService', 'outboundChartService', 'outboundNotificationService', 'Toggle', campaignCommandsPaneCtrl],
			link: postlink
		};

		function campaignCommandsPaneCtrl($scope, $state, outboundService, outboundChartService, toggleService, outboundNotificationService) {
			$scope.manualPlanSwitch = false;
			$scope.manualBacklogSwitch = false;
			$scope.isPlanClickedSave = false;
			$scope.isBacklogClickedSave = false;
			$scope.isPlanClickedReset = false;
			$scope.isBacklogClickedReset = false;
			$scope.manualPlanInput = null;
			$scope.manualBacklogInput = null;

			$scope.addManualPlan = addManualPlan;
			$scope.removeManualPlan = removeManualPlan;
			$scope.addManualBacklog = addManualBacklog;
			$scope.removeManualBacklog = removeManualBacklog;
			$scope.replan = replan;
			$scope.validManualProductionPlan = validManualProductionPlan;
			$scope.isPlanTryingDoAction = isPlanTryingDoAction;
			$scope.isBacklogTryingDoAction = isBacklogTryingDoAction;
			$scope.validManualBacklog = validManualBacklog;
			$scope.showDateSelectionHint = showDateSelectionHint;

			$scope.gotoEditCampaign = function () {
				if ($scope.campaign)
					$state.go('outbound.edit', { Id: $scope.campaign.Id });
			};

			$scope.toggleManualPlan = function() {
				$scope.manualPlanSwitch = !$scope.manualPlanSwitch;
				$scope.manualBacklogSwitch = false;
				resetActionFlag();
			}

			$scope.toggleManualBacklog = function() {
				$scope.manualBacklogSwitch = !$scope.manualBacklogSwitch;
				$scope.manualPlanSwitch = false;
				resetActionFlag();
			}

			function unclosedDays() {
				return $scope.selectedDates.filter(function (d) {
					return $scope.selectedDatesClosed.indexOf(d) < 0;
				});
			}

			function callbackDone() {
				$scope.isLoading = true;
				resetActionFlag();
			}

			function resetActionFlag() {
				$scope.isPlanClickedSave = false;
				$scope.isBacklogClickedSave = false;
				$scope.isPlanClickedReset = false;
				$scope.isBacklogClickedReset = false;
			}

			function addManualPlan() {
				$scope.isPlanClickedSave = true;
				if (showDateSelectionHint()) {
					 return;
				}

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
				$scope.isPlanClickedReset = true;
				if ($scope.selectedDates.length == 0) return;

					$scope.isLoading = true;
					outboundChartService.removeManualPlan({
						campaignId: $scope.campaign.Id,
						selectedDates: $scope.selectedDates
					}, function(response) {
						if (angular.isDefined($scope.callbacks.removeManualPlan)) {
							$scope.callbacks.removeManualPlan(response, callbackDone);
						} else {
							callbackDone();
						}
					});

			}

			function addManualBacklog() {
				$scope.isBacklogClickedSave = true;
				if (showDateSelectionHint()) {
					return;
				}

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
				$scope.isBacklogClickedReset = true;
				if ($scope.selectedDates.length == 0) return;

					$scope.isLoading = true;
					outboundChartService.removeActualBacklog({
						campaignId: $scope.campaign.Id,
						selectedDates: $scope.selectedDates
					}, function(response) {
						if (angular.isDefined($scope.callbacks.removeManualBacklog)) {
							$scope.callbacks.removeManualBacklog(response, callbackDone);
						} else {
							callbackDone();
						}
					});
			}

			function replan() {
				$scope.isLoading = true;
				var selectedDates = toggleService.Wfm_Outbound_ReplanAfterScheduled_43752 ? $scope.selectedDates : [];
				
				outboundChartService.replan({
					campaignId: $scope.campaign.Id,
					selectedDates: selectedDates
				}, function (response) {
					if (angular.isDefined($scope.callbacks.replan)) {
						$scope.callbacks.replan(response, callbackDone);
					} else {
						callbackDone();
					}
				});
			}

			function validManualProductionPlan() {
				if ($scope.manualPlanInput == null) return false;
				else return $scope.manualPlanInput >= 0;
			}

			function isPlanTryingDoAction() {
				return $scope.isPlanClickedSave || $scope.isPlanClickedReset;
			}

			function isBacklogTryingDoAction() {
				return $scope.isBacklogClickedSave || $scope.isBacklogClickedReset;
			}

			function validManualBacklog() {
				if ($scope.manualBacklogInput == null) return false;
				else return $scope.manualBacklogInput >= 0;
			}

			function showDateSelectionHint() {
				if ($scope.selectedDates && $scope.selectedDates.length > 0) return false;
				return true;
			}
		}

		function postlink(scope, elem, attrs) {
		}
	}

})();
