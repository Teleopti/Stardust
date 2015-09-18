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
				selecteDates: '='
			},
			controller: ['$scope', '$state', campaignCommandsPaneCtrl],
			link: postlink
		};

		function campaignCommandsPaneCtrl($scope, $state) {
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
		
			function addManualPlan() {
				

			}

			function removeManualPlan() {
				

			}

			function addManualBacklog() {
				
			}

			function removeManualBacklog() {
				
			}

			function replan() {
				

			}


			function validManualProductionPlan() {
				
			}

			function validManualBacklog() {
				

			}

			function showDateSelectionHint() {
				

			}

		}

		function postlink(scope, elem, attrs) {
			
		


		}

		
	}

})();