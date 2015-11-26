(function () {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('ResourceplannerFilterCtrl', [
			'$scope', 'ResourcePlannerFilterSrvc', 'ResourcePlannerSvrc','$state','$stateParams', function ($scope, ResourcePlannerFilterSrvc, ResourcePlannerSvrc,$state,$stateParams) {
				var maxHits = 5;
				$scope.name = ""
				$scope.isEnabled = true;
				$scope.results = [];
				$scope.default = false;
				$scope.selectedResults = [];
				$scope.filterId = "";
				$scope.dayOffsPerWeek = {
					MinDayOffsPerWeek: 2,
					MaxDayOffsPerWeek: 3
				};
				$scope.consecDaysOff = {
					MinConsecDaysOff: 2,
					MaxConsecDaysOff: 3
				};
				$scope.consecWorkDays = {
					MinConsecWorkDays: 2,
					MaxConsecWorkDays:3
				};
				$scope.isSearching=false;
				if (Object.keys($stateParams.filterId).length > 0) {
					ResourcePlannerSvrc.getFilter.query({id:$stateParams.filterId}).$promise.then(function(result){
						$scope.name = result.Name;
						$scope.filterId = $stateParams.filterId;
						$scope.default = result.Default;
						$scope.selectedResults = result.Filters
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
							MaxConsecWorkDays:result.MaxConsecutiveWorkdays
						}
					});
				}

				$scope.$watch(function() { return $scope.searchString; },
					function (input) {
						if (input === '' || input === undefined) {
							$scope.results = [];
							return;
						}

						$scope.isSearching = true;
						ResourcePlannerFilterSrvc.getData({ searchString: input, maxHits: maxHits }).then(function (results) {
							$scope.results = results.data;
							$scope.isSearching = false;
						}, function() {
							$scope.isSearching = false;
						});
					}
				);
				$scope.isValid = function () {
					return $scope.isValidDayOffsPerWeek() &&
						$scope.isValidConsecDaysOff() &&
						$scope.isValidConsecDaysOff() &&
						$scope.isValidFilters() &&
						$scope.isValidName();

				}

				$scope.isValidDayOffsPerWeek = function () {
					return $scope.dayOffsPerWeek.MinDayOffsPerWeek <= $scope.dayOffsPerWeek.MaxDayOffsPerWeek;
				}

				$scope.isValidConsecDaysOff = function () {
					return $scope.consecDaysOff.MinConsecDaysOff <= $scope.consecDaysOff.MaxConsecDaysOff;
				}
				$scope.clearInput = function () {
					$scope.searchString = '';
					$scope.results = [];
				}

				$scope.isValidConsecWorkDays = function () {
					return $scope.consecWorkDays.MinConsecWorkDays <= $scope.consecWorkDays.MaxConsecWorkDays;
				}

				$scope.isValidFilters = function () {
					return $scope.selectedResults.length > 0;
				}
				$scope.isValidName = function(){
					return $scope.name.length >0;
				}

				$scope.selectResultItem = function (item) {
					if ($scope.selectedResults.indexOf(item) < 0) {
						$scope.selectedResults.push(item);
					}
					$scope.clearInput();
				}

				$scope.moreResultsExists = function () {
					return $scope.results.length >= maxHits;
				}
				$scope.removeNode = function(node){
					var p = $scope.selectedResults.indexOf(node);
					$scope.selectedResults.splice(p,1);
				}

				$scope.persist = function () {
					if ($scope.isValid()) {
						$scope.isEnabled = false;
						ResourcePlannerSvrc.saveDayoffRules.update(
						{
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
						}).$promise.then(function(){
							$state.go('resourceplanner.planningperiod', { id: $stateParams.periodId });
						});
					}
				}
			}
		]);
})();
