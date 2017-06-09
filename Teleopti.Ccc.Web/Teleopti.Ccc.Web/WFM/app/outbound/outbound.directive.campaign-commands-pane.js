(function() {

	'use strict';

	angular.module('wfm.outbound')
		.directive('campaignCommandsPane', campaignCommandsPaneDirective);

	function campaignCommandsPaneDirective() {
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
			controller: campaignCommandsPaneCtrl,
			controllerAs: 'vm',
			bindToController: true,
		};
	}

	campaignCommandsPaneCtrl.$inject = ['$scope','$state', 'outboundChartService', 'outboundNotificationService', 'Toggle'];

	function campaignCommandsPaneCtrl($scope, $state, outboundChartService, outboundNotificationService, toggleService) {
		var vm = this;

		vm.manualPlanSwitch = false;
		vm.manualBacklogSwitch = false;
		vm.isPlanClickedSave = false;
		vm.isBacklogClickedSave = false;
		vm.isPlanClickedReset = false;
		vm.isBacklogClickedReset = false;
		vm.manualPlanInput = null;
		vm.manualBacklogInput = null;
		vm.ignoredDates = [];

		vm.addManualPlan = addManualPlan;
		vm.removeManualPlan = removeManualPlan;
		vm.addManualBacklog = addManualBacklog;
		vm.removeManualBacklog = removeManualBacklog;
		vm.replan = replan;
		vm.validManualProductionPlan = validManualProductionPlan;
		vm.isPlanTryingDoAction = isPlanTryingDoAction;
		vm.isBacklogTryingDoAction = isBacklogTryingDoAction;
		vm.validManualBacklog = validManualBacklog;
		vm.showDateSelectionHint = showDateSelectionHint;

		vm.showIgnoreSchedule = function() {
			return toggleService.Wfm_Outbound_ReplanAfterScheduled_43752;
		};

		vm.enableIgnoreSchedule = function() {
			if (!vm.campaign.IsScheduled || vm.selectedDates.length === 0)
				return false;

			var graphData = vm.campaign.graphData;
			return vm.selectedDates.some(function (selectedDate) {
				var index = graphData.dates.indexOf(selectedDate);
				return graphData.schedules[index] != 0;
			});
		};

		vm.ignoreSchedule = function () {
			vm.manualBacklogSwitch = false;
			vm.manualPlanSwitch = false;
			vm.ignoredDates = angular.copy(vm.selectedDates);

			vm.callbacks.ignoreSchedules(vm.ignoredDates, callbackDone);
		};

		vm.gotoEditCampaign = function () {
			if (vm.campaign)
				$state.go('outbound.edit', { Id: vm.campaign.Id });
		};

		vm.toggleManualPlan = function() {
			vm.manualPlanSwitch = !vm.manualPlanSwitch;
			vm.manualBacklogSwitch = false;
			resetActionFlag();
		};

		vm.toggleManualBacklog = function() {
			vm.manualBacklogSwitch = !vm.manualBacklogSwitch;
			vm.manualPlanSwitch = false;
			resetActionFlag();
		};

		function unclosedDays() {
			return vm.selectedDates.filter(function (d) {
				return vm.selectedDatesClosed.indexOf(d) < 0;
			});
		}

		function callbackDone() {
			vm.isLoading = true;
			resetActionFlag();
		}

		function resetActionFlag() {
			vm.isPlanClickedSave = false;
			vm.isBacklogClickedSave = false;
			vm.isPlanClickedReset = false;
			vm.isBacklogClickedReset = false;
		}

		function addManualPlan() {
			vm.isPlanClickedSave = true;
			if (showDateSelectionHint()) {
				 return;
			}

			vm.isLoading = true;
			outboundChartService.updateManualPlan({
				campaignId: vm.campaign.Id,
				selectedDates: unclosedDays(),
				manualPlanInput: vm.manualPlanInput
			}, function (response) {
				if (angular.isDefined(vm.callbacks.addManualPlan)) {
					vm.callbacks.addManualPlan(response, callbackDone);
				} else {
					callbackDone();
				}
			});
		}

		function removeManualPlan() {
			vm.isPlanClickedReset = true;
			if (vm.selectedDates.length == 0) return;

				vm.isLoading = true;
				outboundChartService.removeManualPlan({
					campaignId: vm.campaign.Id,
					selectedDates: vm.selectedDates
				}, function(response) {
					if (angular.isDefined(vm.callbacks.removeManualPlan)) {
						vm.callbacks.removeManualPlan(response, callbackDone);
					} else {
						callbackDone();
					}
				});

		}

		function addManualBacklog() {
			vm.isBacklogClickedSave = true;
			if (showDateSelectionHint()) {
				return;
			}

			vm.isLoading = true;
			outboundChartService.updateBacklog({
				campaignId: vm.campaign.Id,
				selectedDates: vm.selectedDates,
				manualBacklogInput: vm.manualBacklogInput
			}, function (response) {
				if (angular.isDefined(vm.callbacks.addManualBacklog)) {
					vm.callbacks.addManualBacklog(response, callbackDone);
				} else {
					callbackDone();
				}
			});
		}

		function removeManualBacklog() {
			vm.isBacklogClickedReset = true;
			if (vm.selectedDates.length == 0) return;

				vm.isLoading = true;
				outboundChartService.removeActualBacklog({
					campaignId: vm.campaign.Id,
					selectedDates: vm.selectedDates
				}, function(response) {
					if (angular.isDefined(vm.callbacks.removeManualBacklog)) {
						vm.callbacks.removeManualBacklog(response, callbackDone);
					} else {
						callbackDone();
					}
				});
		}

		function replan() {
			vm.selectedDates.sort(function(c, n){
				return moment(c) - moment(n);
			});

			if(!toggleService.Wfm_Outbound_ReplanAfterScheduled_43752){
				vm.ignoredDates = [];
				triggerReplanAction();
			} else if(vm.selectedDates.length == 0){
				vm.ignoredDates = [];
				triggerReplanAction();
			} else {
				vm.ignoredDates = angular.copy(vm.selectedDates);
				triggerReplanAction();
			}
		}

		function triggerReplanAction(){
			vm.isLoading = true;
			outboundChartService.replan({
				campaignId: vm.campaign.Id,
				selectedDates: vm.ignoredDates
			}, function (response) {
				if (angular.isDefined(vm.callbacks.replan)) {
					vm.callbacks.replan(response, callbackDone);
				} else {
					callbackDone();
				}
			});
		}

		function getDatesForReplanning(start, end){
			var dates = [],
				datesLength = moment(end).diff(moment(start), 'days');
			for(var i = 0; i <= datesLength; i++){
				dates.push(moment(start).add(i, 'days').format('YYYY-MM-DD'));
			}
			return dates;
		}


		function validManualProductionPlan() {
			if (vm.manualPlanInput == null) return false;
			else return vm.manualPlanInput >= 0;
		}

		function isPlanTryingDoAction() {
			return vm.isPlanClickedSave || vm.isPlanClickedReset;
		}

		function isBacklogTryingDoAction() {
			return vm.isBacklogClickedSave || vm.isBacklogClickedReset;
		}

		function validManualBacklog() {
			if (vm.manualBacklogInput == null) return false;
			else return vm.manualBacklogInput >= 0;
		}

		function showDateSelectionHint() {
			if (vm.selectedDates && vm.selectedDates.length > 0) return false;
			return true;
		}
	}
})();
