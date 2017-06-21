﻿(function() {

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

		vm.ignoreScheduleSwitched = false;
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

		vm.isToggleScheduleDatesEnabled = function () {
			return vm.campaign.IsScheduled && toggleService.Wfm_Outbound_IgnoreScheduleForReplan_43752;
		};

		vm.enableIgnoreSchedule = function () {
			if (vm.ignoredDates.length > 0) return true;

			if (!vm.campaign.IsScheduled || vm.selectedDates.length === 0)
				return false;

			var graphData = vm.campaign.graphData;
			return vm.selectedDates.some(function (selectedDate) {
				var index = graphData.dates.indexOf(selectedDate);
				return graphData.schedules[index] != 0;
			});
		};

		vm.showIgnoreSchedulesButton = function(){
			return vm.ignoredDates.length == 0 && !vm.ignoreScheduleSwitched;
		};

		vm.ignoreSchedules = function () {
			vm.ignoreScheduleSwitched = true;
			vm.manualBacklogSwitch = false;
			vm.manualPlanSwitch = false;
			vm.ignoredDates = getIgnoredDates();
			vm.isLoading = true;
			vm.callbacks.ignoreSchedules(vm.ignoredDates, callbackDone);
		};

		function getIgnoredDates() {
			var graphData = vm.campaign.graphData;
			return vm.selectedDates.filter(function(date) {
				var index = graphData.dates.indexOf(date);
				return graphData.schedules[index] > 0;
			});
		}

		vm.showAllSchedules = function () {
			vm.ignoreScheduleSwitched = false;
			vm.manualBacklogSwitch = false;
			vm.manualPlanSwitch = false;
			vm.isLoading = true;
			vm.callbacks.showAllSchedules(vm.ignoredDates,
				function() {
					vm.ignoredDates = [];
					callbackDone();
				});
		};

		vm.gotoEditCampaign = function () {
			if (vm.campaign)
				$state.go('outbound.edit', { Id: vm.campaign.Id });
		};

		vm.toggleManualPlan = function() {
			vm.manualPlanSwitch = !vm.manualPlanSwitch;
			vm.manualBacklogSwitch = false;
			vm.ignoreScheduleSwitched = false;
			resetActionFlag();
		};

		vm.toggleManualBacklog = function() {
			vm.manualBacklogSwitch = !vm.manualBacklogSwitch;
			vm.manualPlanSwitch = false;
			vm.ignoreScheduleSwitched = false;
			resetActionFlag();
		};

		function unclosedDays() {
			return vm.selectedDates.filter(function (d) {
				return vm.selectedDatesClosed.indexOf(d) < 0;
			});
		}

		function callbackDone() {
			vm.isLoading = false;
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
				manualPlanInput: vm.manualPlanInput,
				ignoredDates: vm.ignoredDates
			}, function (response) {
				if (angular.isDefined(vm.callbacks.addManualPlan)) {
					vm.callbacks.addManualPlan(response, vm.ignoredDates, callbackDone);
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
				selectedDates: vm.selectedDates,
				ignoredDates: vm.ignoredDates
			}, function(response) {
				if (angular.isDefined(vm.callbacks.removeManualPlan)) {
					vm.callbacks.removeManualPlan(response, vm.ignoredDates, callbackDone);
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
				manualBacklogInput: vm.manualBacklogInput,
				ignoredDates: vm.ignoredDates
			}, function (response) {
				if (angular.isDefined(vm.callbacks.addManualBacklog)) {
					vm.callbacks.addManualBacklog(response, vm.ignoredDates, callbackDone);
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
					selectedDates: vm.selectedDates,
					ignoredDates: vm.ignoredDates
				}, function(response) {
					if (angular.isDefined(vm.callbacks.removeManualBacklog)) {
						vm.callbacks.removeManualBacklog(response, vm.ignoredDates, callbackDone);
					} else {
						callbackDone();
					}
				});
		}

		function replan(){
			vm.isLoading = true;
			outboundChartService.replan({
				campaignId: vm.campaign.Id,
				ignoredDates: vm.ignoredDates
			}, function (response) {
				if (angular.isDefined(vm.callbacks.replan)) {
					vm.callbacks.replan(response, vm.ignoredDates, callbackDone);
				} else {
					callbackDone();
				}
			});
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
