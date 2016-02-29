(function() {
	'use strict';
	angular.module('wfm.intraday')
		.controller('IntradayCtrl', [
			'$scope', '$state', 'intradayService', '$stateParams', '$filter', 'growl',
			function($scope, $state, intradayService, $stateParams, $filter, growl) {

				var autocompleteSkill;
				var autocompleteSkillArea;

				var getAutoCompleteControls = function() {
					var autocompleteSkillDOM = document.querySelector('.autocomplete-skill');
					autocompleteSkill = angular.element(autocompleteSkillDOM).scope();

					var autocompleteSkillAreaDOM = document.querySelector('.autocomplete-skillarea');
					autocompleteSkillArea = angular.element(autocompleteSkillAreaDOM).scope();
				};

				$scope.format = intradayService.formatDateTime;

				$scope.$on('$stateChangeSuccess', function(evt, to, params, from) {
					if (params.newSkillArea == true) {
						reloadSkillAreas();
					}
				});

				$scope.newSkillArea = $stateParams.newSkillArea;

				var reloadSkillAreas = function() {
					intradayService.getSkillAreas.query()
						.$promise.then(function (result) {
							getAutoCompleteControls();
							$scope.skillAreas = $filter('orderBy')(result.SkillAreas, 'Name');
							$scope.HasPermissionToModifySkillArea = result.HasPermissionToModifySkillArea;
							if ($scope.skillAreas.length > 0) {
								$scope.selectedItem = $scope.skillAreas[0];
								if (autocompleteSkillArea)
									autocompleteSkillArea.selectedSkillArea = $scope.selectedItem;
							}
								
							intradayService.getSkills.query().
								$promise.then(function (result) {
									$scope.skills = result;
									if (!$scope.selectedItem) {
										$scope.selectedItem = $scope.skills[0];
										if (autocompleteSkill)
											autocompleteSkill.selectedSkill = $scope.selectedItem;
									}
								});
						});
				};

				reloadSkillAreas();
				
				$scope.configMode = function () {
					$state.go('intraday.config', { isNewSkillArea: false });
				};

				$scope.modalShown = false;

				$scope.toggleModal = function() {
					$scope.modalShown = !$scope.modalShown;
				};

				$scope.deleteSkillArea = function(skillArea) {
					intradayService.deleteSkillArea.remove(
						{
							id: skillArea.Id
						})
						.$promise.then(function(result) {
							$scope.skillAreas.splice($scope.skillAreas.indexOf(skillArea), 1);
							$scope.selectedItem = null;
							clearSkillAreaSelection();
							notifySkillAreaDeletion();
						});

					$scope.toggleModal();
				};

				var notifySkillAreaDeletion = function () {
				    growl.success("<i class='mdi mdi-thumb-up'></i> Deleted Area", {
				        ttl: 5000,
				        disableCountDown: true
				    });
				};

				$scope.querySearch = function(query, myArray) {
					var results = query ? myArray.filter(createFilterFor(query)) : myArray, deferred;
					return results;
				};

				function createFilterFor(query) {
					var lowercaseQuery = angular.lowercase(query);
					return function filterFn(item) {
						var lowercaseName = angular.lowercase(item.Name);
						return (lowercaseName.indexOf(lowercaseQuery) === 0);
					};
				};

				$scope.selectedSkillChange = function(item) {
					if (this.selectedSkill) {
						$scope.skillSelected(item);
					}
				};

				$scope.skillSelected = function (item) {
					$scope.selectedItem = item;
					clearSkillAreaSelection();

					intradayService.getSkillMonitorData.query(
						{
							id: $scope.selectedItem.Id
						})
						.$promise.then(function (result) {
							$scope.forecastedCalls = result.ForecastedCalls;
							$scope.offeredCalls = result.OfferedCalls;
							$scope.latestStatsTime = $filter('date')(result.LatestStatsTime, 'shortTime');
							$scope.difference = result.ForecastedActualCallsDiff;
							$scope.HasMonitorData = true;
						});
				};

				$scope.selectedSkillAreaChange = function(item) {
					if (this.selectedSkillArea) {
						$scope.skillAreaSelected(item);
					}
				};

				$scope.skillAreaSelected = function(item) {
					$scope.selectedItem = item;
					clearSkillSelection();

					intradayService.getSkillAreaMonitorData.query(
						{
							id: $scope.selectedItem.Id
						})
						.$promise.then(function (result) {
						if (moment(result.LatestStatsTime).isSame(moment('0001-01-01'))) {
							$scope.HasMonitorData = false;
							return;
						}
						$scope.forecastedCalls = result.ForecastedCalls;
						$scope.offeredCalls = result.OfferedCalls;
						$scope.latestStatsTime = $filter('date')(result.LatestStatsTime, 'shortTime');
						$scope.difference = result.ForecastedActualCallsDiff;
						$scope.HasMonitorData = true;
					});
				};


				function clearSkillSelection() {
					if (!autocompleteSkill) return;
					
					autocompleteSkill.selectedSkill = null;
					autocompleteSkill.searchSkillText = '';
				};

				function clearSkillAreaSelection() {
					if (!autocompleteSkillArea) return;
					
					autocompleteSkillArea.selectedSkillArea = null;
					autocompleteSkillArea.searchSkillAreaText = '';
				};
			}
		]);
})();
