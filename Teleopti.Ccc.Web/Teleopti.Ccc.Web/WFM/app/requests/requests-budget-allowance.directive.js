﻿"use strict";
(function () {

	function requestsBudgetAllowanceController($scope) {
		var vm = this;
		vm.budgetGroups = [];
		vm.budgetAllowanceList = [];
		vm.selectedDate = $scope.selectedDate;
		vm.selectedBudgetGroupId = $scope.selectedBudgetGroupId;
		vm.allowanceOption = $scope.allowanceOption;

		function setDefaultOptions() {
			// Demo data
			vm.selectedDate = moment().toDate();
			vm.allowanceOption = "TotalAllowance";
			vm.budgetGroups = [{ id: 1, name: "Backoffice" }, { id: 2, name: "Frontoffice" }];
			vm.selectedBudgetGroupId = vm.budgetGroups[0].id;
		}

		vm.loadBudgetAllowance = function () {
			// TODO: Load budget allowance from server side

			vm.budgetAllowanceList = [];
			for (var i = 0; i < 7; i++) {
				var currentDate = moment(vm.selectedDate).startOf("week").add(i, "days");
				var weekday = currentDate.format("dddd");
				var allowance = {
					date: currentDate,
					allowance: 0.00,
					usedTotal: 0.00,
					absoluteDifference: 0.00,
					relativeDifference: 0.00,
					isWeekend: weekday === "Saturday" || weekday === "Sunday"
				};
				vm.budgetAllowanceList.push(allowance);
			}
		}

		function initialize() {
			setDefaultOptions();
			vm.loadBudgetAllowance();

			$scope.selectedDate = vm.selectedDate;
			$scope.selectedBudgetGroupId = vm.selectedBudgetGroupId;
			$scope.allowanceOption = vm.allowanceOption;
			$scope.budgetGroups = vm.budgetGroups;
			$scope.budgetAllowanceList = vm.budgetAllowanceList;
			$scope.loadBudgetAllowance = vm.loadBudgetAllowance;
		}

		initialize();
	}

	var requestsBudgetAllowanceDirective = function () {
		return {
			scope: {
				selectedDate: "=?",
				selectedBudgetGroupId: "=?",
				allowanceOption: "=?",
				budgetGroups: "=?",
				budgetAllowanceList: "=?",
				loadBudgetAllowance: "&"
			},
			restrict: "E",
			controller: "requestsBudgetAllowanceCtrl",
			controllerAs: "vm",
			bindToController: true,
			templateUrl: "app/requests/html/requests-budget-allowance.tpl.html",
			link: link
		};

		function link(scope, elem, attrs, ctrls) {
			//var vm = scope.vm;
		}
	};

	angular.module("wfm.requests")
        .controller("requestsBudgetAllowanceCtrl", ["$scope", requestsBudgetAllowanceController])
        .directive("requestsBudgetAllowance", requestsBudgetAllowanceDirective);
})();