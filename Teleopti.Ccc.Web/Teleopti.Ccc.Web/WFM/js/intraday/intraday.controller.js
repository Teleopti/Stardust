(function() {
	'use strict';
	angular.module('wfm.intraday')
		.controller('IntradayCtrl', [
			'$scope', '$state', 'intradayService', '$filter', 'growl', '$timeout',
			function ($scope, $state, intradayService, $filter, growl, $timeout) {

				var autocompleteSkill;
				var autocompleteSkillArea;
				var timeoutPromise;
				var pollingTimeout = 5000;

				var getAutoCompleteControls = function() {
					var autocompleteSkillDOM = document.querySelector('.autocomplete-skill');
					autocompleteSkill = angular.element(autocompleteSkillDOM).scope();

					var autocompleteSkillAreaDOM = document.querySelector('.autocomplete-skillarea');
					autocompleteSkillArea = angular.element(autocompleteSkillAreaDOM).scope();
				};

				$scope.format = intradayService.formatDateTime;


				$scope.$on('$stateChangeSuccess', function (evt, to, params, from) {
					if (params.isNewSkillArea == true) {
						reloadSkillAreas(params.isNewSkillArea);
					}
				});

				var reloadSkillAreas = function(isNew) {
					intradayService.getSkillAreas.query()
						.$promise.then(function (result) {
							getAutoCompleteControls();
							$scope.skillAreas = $filter('orderBy')(result.SkillAreas, 'Name');
							if (isNew) $scope.latest = $filter('orderBy')(result.SkillAreas, 'created_at', true);
							$scope.HasPermissionToModifySkillArea = result.HasPermissionToModifySkillArea;

							intradayService.getSkills.query().
								$promise.then(function (result) {
									$scope.skills = result;
									if ($scope.skillAreas.length === 0) {
										$scope.selectedItem = $scope.skills[0];
										if (autocompleteSkill) {
											autocompleteSkill.selectedSkill = $scope.selectedItem;
										}
									}
									if ($scope.skillAreas.length > 0) {
										if (isNew) {
											$scope.selectedItem = $scope.latest[0];
											if (autocompleteSkillArea)
												autocompleteSkillArea.selectedSkillArea = $scope.selectedItem;
										} else {
											$scope.selectedItem = $scope.skillAreas[0];
											if (autocompleteSkillArea)
												autocompleteSkillArea.selectedSkillArea = $scope.selectedItem;
										}
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
					pollSkillMonitorData();
				};

				
				var pollSkillMonitorData = function () {
					cancelTimeout();

					intradayService.getSkillMonitorData.query(
						{
							id: $scope.selectedItem.Id
						})
						.$promise.then(function (result) {
							timeoutPromise = $timeout(pollSkillMonitorData, pollingTimeout);
							if (moment(result.LatestStatsTime).isSame(moment('0001-01-01'))) {
								$scope.HasMonitorData = false;
								return;
							}
							$scope.forecastedCalls = $filter('number')(result.ForecastedCalls, 1);
							$scope.offeredCalls = $filter('number')(result.OfferedCalls, 1);
							$scope.latestStatsTime = $filter('date')(result.LatestStatsTime, 'shortTime');
							$scope.difference = $filter('number')(result.ForecastedActualCallsDiff, 1);
							$scope.HasMonitorData = true;
						},
						function(error) {
							timeoutPromise = $timeout(pollSkillMonitorData, pollingTimeout);
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
					pollSkillAreaMonitorData();
				};

				var pollSkillAreaMonitorData = function () {
					cancelTimeout();
					intradayService.getSkillAreaMonitorData.query(
						{
							id: $scope.selectedItem.Id
						})
						.$promise.then(function (result) {
							timeoutPromise = $timeout(pollSkillAreaMonitorData, pollingTimeout);
							if (moment(result.LatestStatsTime).isSame(moment('0001-01-01'))) {
								$scope.HasMonitorData = false;
								return;
							}
							$scope.forecastedCalls = $filter('number')(result.ForecastedCalls, 1);
							$scope.offeredCalls = $filter('number')(result.OfferedCalls, 1);
							$scope.latestStatsTime = $filter('date')(result.LatestStatsTime, 'shortTime');
							$scope.difference = $filter('number')(result.ForecastedActualCallsDiff, 1);
							$scope.HasMonitorData = true;
						},
						function(error) {
							timeoutPromise = $timeout(pollSkillAreaMonitorData, pollingTimeout);
						});
				}

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

				$scope.$on("$destroy", function (event) {
	               cancelTimeout();
            });

				$scope.$on('$locationChangeStart', function () {
					cancelTimeout();
				});

				var cancelTimeout = function () {
					if (timeoutPromise)
						$timeout.cancel(timeoutPromise);
				};
			}
		]);
})();
