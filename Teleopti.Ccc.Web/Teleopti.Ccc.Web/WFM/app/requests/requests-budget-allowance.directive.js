"use strict";
(function () {
	function requestsBudgetAllowanceController($translate, $filter, requestsDataSvc) {
		var vm = this;
		vm.isLoading = false;
		vm.budgetGroups = [];
		vm.budgetAllowanceList = [];
		vm.linkedAbsences = [];
		vm.selectedDate = new Date();
		vm.previousSelectedDate = vm.selectedDate;
		vm.selectedBudgetGroupId = "";

		vm.formatAbsenceName = function (absence) {
			var template = $translate.instant("UsedBy");
			return template.replace("{0}", absence);
		}

		vm.getStyle = function (allowance) {
			if (moment(vm.selectedDate).isSame(allowance.date, "day")) return "current-date";
			return allowance.isWeekend ? "weekend" : "";
		}

		vm.selectedDateChanged = function () {
			if (moment(vm.previousSelectedDate).isSame(moment(vm.selectedDate), "week")) {
				vm.previousSelectedDate = vm.selectedDate;
				return;
			}

			vm.previousSelectedDate = vm.selectedDate;
			vm.loadBudgetAllowance();
		}

		vm.loadBudgetAllowance = function () {
			if (vm.budgetGroups.length === 0) return;

			requestsDataSvc.getBudgetAllowancePromise(moment(vm.selectedDate).format("YYYY-MM-DD"), vm.selectedBudgetGroupId)
				.then(function (response) {
					vm.budgetAllowanceList = [];
					if (response.data.length === 0) return;

					// Get absences linked to this budget group
					vm.linkedAbsences = [];
					var firstAllowance = response.data[0];
					for (var absenceAllowance in firstAllowance.UsedAbsencesDictionary) {
						if (absenceAllowance.indexOf("$") !== 0 &&
							firstAllowance.UsedAbsencesDictionary.hasOwnProperty(absenceAllowance) &&
							typeof (firstAllowance.UsedAbsencesDictionary[absenceAllowance]) === "number") {
							vm.linkedAbsences.push(absenceAllowance);
						}
					}

					var fractionSize = 2;
					var numberFilter = $filter("number");
					for (var i = 0; i < response.data.length; i++) {
						var allowance = response.data[i];

						var relativeDifference = allowance.RelativeDifference !== null
							? numberFilter(allowance.RelativeDifference * 100, fractionSize)
							: "-";
						if (relativeDifference !== "∞" && relativeDifference !== "-") {
							relativeDifference = relativeDifference + "%";
						}

						var allowanceModel = {
							date: moment(allowance.Date),
							fullAllowance: numberFilter(allowance.FullAllowance, fractionSize),
							shrinkedAllowance: numberFilter(allowance.ShrinkedAllowance, fractionSize),
							usedTotal: numberFilter(allowance.UsedTotalAbsences, fractionSize),
							absoluteDifference: numberFilter(allowance.AbsoluteDifference, fractionSize),
							relativeDifference: relativeDifference,
							totalHeadCounts: numberFilter(allowance.TotalHeadCounts, fractionSize),
							isWeekend: allowance.IsWeekend
						};

						for (var j = 0; j < vm.linkedAbsences.length; j++) {
							var absenceName = vm.linkedAbsences[j];
							allowanceModel[absenceName] = numberFilter(allowance.UsedAbsencesDictionary[absenceName], 2);
						}

						vm.budgetAllowanceList.push(allowanceModel);
					}

					vm.isLoading = false;
				});
		}

		function initialize() {
			vm.isLoading = true;
			requestsDataSvc.getBudgetGroupsPromise().then(function (response) {
				if (response.data.length > 0) {
					vm.budgetGroups = response.data;
					vm.selectedBudgetGroupId = vm.budgetGroups[0].Id;
					vm.loadBudgetAllowance();
				} else {
					vm.budgetGroups = [];
					vm.isLoading = false;
				}
			});
		}

		initialize();
	}

	var requestsBudgetAllowanceDirective = function () {
		return {
			restrict: "E",
			controller: "requestsBudgetAllowanceCtrl",
			controllerAs: "vm",
			bindToController: true,
			templateUrl: "app/requests/html/requests-budget-allowance.tpl.html"
		};
	};

	angular.module("wfm.requests")
		.controller("requestsBudgetAllowanceCtrl", ["$translate", "$filter", "requestsDataService", requestsBudgetAllowanceController])
		.directive("requestsBudgetAllowance", requestsBudgetAllowanceDirective);
})();