(function () {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('ResourceplannerFilterCtrl', [
			'$scope', 'ResourcePlannerFilterSrvc', '$state', '$stateParams', 'NoticeService', 'dayOffRuleService',
			function ($scope, ResourcePlannerFilterSrvc, $state, $stateParams, NoticeService, dayOffRuleService) {
				var maxHits = 100;
				$scope.name = "";
				$scope.isEnabled = true;
				$scope.selectedItem=undefined;
				$scope.results = [];
				$scope.default = false;
				$scope.selectedResults = [];
				$scope.filterId = "";
				$scope.dayOffsPerWeek = {
					MinDayOffsPerWeek: 1,
					MaxDayOffsPerWeek: 3
				};
				$scope.consecDaysOff = {
					MinConsecDaysOff: 1,
					MaxConsecDaysOff: 3
				};
				$scope.consecWorkDays = {
					MinConsecWorkDays: 2,
					MaxConsecWorkDays: 6
				};
				$scope.isSearching = false;
				if (Object.keys($stateParams.filterId).length > 0) {
					if ($stateParams.isDefault) {
						$scope.default = $stateParams.isDefault;
						//translate
						$scope.name = "Default";
						$scope.filterId = $stateParams.filterId;
						if ($scope.filterId !== '00000000-0000-0000-0000-000000000000') {
							dayOffRuleService.getDayOffRule({ id: $stateParams.filterId })
								.$promise.then(function (result) {
									$scope.dayOffsPerWeek = {
										MinDayOffsPerWeek: result.MinDayOffsPerWeek,
										MaxDayOffsPerWeek: result.MaxDayOffsPerWeek
									};
									$scope.consecDaysOff = {
										MinConsecDaysOff: result.MinConsecutiveDayOffs,
										MaxConsecDaysOff: result.MaxConsecutiveDayOffs
									};
									$scope.consecWorkDays = {
										MinConsecWorkDays: result.MinConsecutiveWorkdays,
										MaxConsecWorkDays: result.MaxConsecutiveWorkdays
									}
								});
						}
					} else {
						dayOffRuleService.getDayOffRule({ id: $stateParams.filterId })
							.$promise.then(function (result) {
								$scope.name = result.Name;
								$scope.filterId = $stateParams.filterId;
								$scope.default = result.Default;
								$scope.selectedResults = result.Filters;
								$scope.dayOffsPerWeek = {
									MinDayOffsPerWeek: result.MinDayOffsPerWeek,
									MaxDayOffsPerWeek: result.MaxDayOffsPerWeek
								};
								$scope.consecDaysOff = {
									MinConsecDaysOff: result.MinConsecutiveDayOffs,
									MaxConsecDaysOff: result.MaxConsecutiveDayOffs
								};
								$scope.consecWorkDays = {
									MinConsecWorkDays: result.MinConsecutiveWorkdays,
									MaxConsecWorkDays: result.MaxConsecutiveWorkdays
								}
							});
					}
				};
				$scope.query = function (input) {
					if (input === "") {
						return;
					};
					$scope.isSearching = true;
					ResourcePlannerFilterSrvc.getData({
						searchString: input,
						maxHits: maxHits
					}).then(function (results) {
						$scope.results = results.data;
						$scope.isSearching = false;
					}, function () {
						$scope.isSearching = false;
					});
				}
				$scope.isValid = function () {
					return $scope.isValidDayOffsPerWeek() &&
						$scope.isValidConsecDaysOff() &&
						$scope.isValidConsecDaysOff() &&
						$scope.isValidFilters() &&
						$scope.isValidName();

				};
				$scope.isValidDayOffsPerWeek = function () {
					return $scope.dayOffsPerWeek.MinDayOffsPerWeek <= $scope.dayOffsPerWeek.MaxDayOffsPerWeek;
				};

				$scope.isValidConsecDaysOff = function () {
					return $scope.consecDaysOff.MinConsecDaysOff <= $scope.consecDaysOff.MaxConsecDaysOff;
				};
				$scope.clearInput = function () {
					$scope.searchString = '';
					$scope.results = [];
				};

				$scope.isValidConsecWorkDays = function () {
					return $scope.consecWorkDays.MinConsecWorkDays <= $scope.consecWorkDays.MaxConsecWorkDays;
				};

				$scope.isValidFilters = function () {
					return $scope.selectedResults.length > 0 || $scope.default;
				};
				$scope.isValidName = function () {
					return $scope.name.length > 0;
				};

				var isVaildUnit = function (item) {
					var check = true;
					$scope.selectedResults.forEach(function (node) {
						if (node.Id === item.Id) {
							check = false;
						};
					});
					return check;
				};
				$scope.selectResultItem = function (item) {
					if (item === null) {
						return;
					}
					if (isVaildUnit(item)) {
						$scope.selectedResults.push(item);
						$scope.clearInput();
					} else {
						$scope.clearInput();
						NoticeService.warning("Unit already exists", 5000, true);
					}
				};

				$scope.moreResultsExists = function () {
					return $scope.results.length >= maxHits;
				};
				$scope.noResultsExists = function () {
					return $scope.results.length === 0 && $scope.searchString.length > 0;
				};
				$scope.removeNode = function (node) {
					var p = $scope.selectedResults.indexOf(node);
					$scope.selectedResults.splice(p, 1);
				};

				$scope.persist = function () {
					if ($scope.isValid()) {
						$scope.isEnabled = false;
						dayOffRuleService.saveDayOffRule({
							MinDayOffsPerWeek: $scope.dayOffsPerWeek.MinDayOffsPerWeek,
							MaxDayOffsPerWeek: $scope.dayOffsPerWeek.MaxDayOffsPerWeek,
							MinConsecutiveWorkdays: $scope.consecWorkDays.MinConsecWorkDays,
							MaxConsecutiveWorkdays: $scope.consecWorkDays.MaxConsecWorkDays,
							MinConsecutiveDayOffs: $scope.consecDaysOff.MinConsecDaysOff,
							MaxConsecutiveDayOffs: $scope.consecDaysOff.MaxConsecDaysOff,
							Id: $scope.filterId,
							Name: $scope.name,
							Default: $scope.default,
							Filters: $scope.selectedResults
						}).$promise.then(function () {
							returnFromCreate();
						});
					}
				}
				$scope.cancelCreate = returnFromCreate;

				function returnFromCreate() {
					if ($stateParams.groupId) {
						$state.go('resourceplanner.oneagentroup',
						{
							groupId: $stateParams.groupId
						});
					} else {
						$state.go('resourceplanner.planningperiod',
						{
							id: $stateParams.periodId
						});
					}
				};
			}
		]);
})();
