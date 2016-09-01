(function() {
	'use strict';
	angular.module('wfm.intraday')
		.controller('IntradayAreaCtrl', [
			'$scope', '$state', 'intradayService', '$filter', 'NoticeService', '$interval', '$timeout', '$compile', '$translate', 'intradayTrafficService', 'intradayPerformanceService', 'intradayMonitorStaffingService',
			function($scope, $state, intradayService, $filter, NoticeService, $interval, $timeout, $compile, $translate, intradayTrafficService, intradayPerformanceService, intradayMonitorStaffingService) {

				var autocompleteSkill;
				var autocompleteSkillArea;
				var timeoutPromise;
				var pollingTimeout = 60000;
				var activeTab = 0;
				$scope.DeleteSkillAreaModal = false;
				$scope.prevArea;
				$scope.drillable;
				var message = $translate.instant('WFMReleaseNotificationWithoutOldModuleLink')
					.replace('{0}', $translate.instant('Intraday'))
					.replace('{1}', "<a href=' http://www.teleopti.com/wfm/customer-feedback.aspx' target='_blank'>")
					.replace('{2}', '</a>');
				var noDataMessage = $translate.instant('NoDataAvailable');
				var prevSkill = {};
				$scope.currentInterval = [];
				$scope.format = intradayService.formatDateTime;
				$scope.viewObj;

				NoticeService.info(message, null, true);


				var getAutoCompleteControls = function() {
					var autocompleteSkillDOM = document.querySelector('.autocomplete-skill');
					autocompleteSkill = angular.element(autocompleteSkillDOM).scope();

					var autocompleteSkillAreaDOM = document.querySelector('.autocomplete-skillarea');
					autocompleteSkillArea = angular.element(autocompleteSkillAreaDOM).scope();
				};

				$scope.openSkillFromArea = function(item) {
					prevSkill = item;
					autocompleteSkill.selectedSkill = item;
					$scope.drillable = true;
				};

				$scope.openSkillAreaFromSkill = function() {
					autocompleteSkillArea.selectedSkillArea = $scope.prevArea;
					$scope.drillable = false;
				};

				$scope.skillSelected = function(item) {
					$scope.selectedItem = item;
					clearSkillAreaSelection();
					pollActiveTabData(activeTab);
				};

				$scope.skillAreaSelected = function(item) {
					$scope.selectedItem = item;
					clearSkillSelection();
					pollActiveTabData(activeTab);
				};

				$scope.deleteSkillArea = function(skillArea) {
					cancelTimeout();
					intradayService.deleteSkillArea.remove({
							id: skillArea.Id
						})
						.$promise.then(function(result) {
							$scope.skillAreas.splice($scope.skillAreas.indexOf(skillArea), 1);
							$scope.selectedItem = null;
							$scope.hasMonitorData = false;
							clearSkillAreaSelection();
							notifySkillAreaDeletion();
						});

					$scope.toggleModal();
				};

				var clearPrev = function() {
					$scope.drillable = false;
					$scope.prevArea = false;
					prevSkill = false;
				}

				$scope.selectedSkillChange = function(item) {
					if (item) {
						$scope.skillSelected(item);
						intradayTrafficService.pollSkillData(item);
						$scope.hiddenArray = [];

						if (!(prevSkill === autocompleteSkill.selectedSkill)) {
							clearPrev();
						}
					}
				};

				$scope.selectedSkillAreaChange = function(item) {
					if (item) {
						$scope.skillAreaSelected(item);
						intradayTrafficService.pollSkillAreaData(item);
						$scope.hiddenArray = [];
						$scope.prevArea = autocompleteSkillArea.selectedSkillArea;
					}
					if ($scope.drillable === true && autocompleteSkillArea.selectedSkillArea) {
						$scope.drillable = false;
					}
				};

				var reloadSkillAreas = function(isNew) {
					intradayService.getSkillAreas.query()
						.$promise.then(function(result) {
							getAutoCompleteControls();
							$scope.skillAreas = $filter('orderBy')(result.SkillAreas, 'Name');
							if (isNew) $scope.latest = $filter('orderBy')(result.SkillAreas, 'created_at', true);
							$scope.HasPermissionToModifySkillArea = result.HasPermissionToModifySkillArea;

							intradayService.getSkills.query().
							$promise.then(function(result) {
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

				function clearSkillSelection() {
					if (!autocompleteSkill) return;
					autocompleteSkill.selectedSkill = null;
					autocompleteSkill.searchSkillText = '';
					$scope.drillable = false;
				};

				function clearSkillAreaSelection() {
					if (!autocompleteSkillArea) return;
					autocompleteSkillArea.selectedSkillArea = null;
					autocompleteSkillArea.searchSkillAreaText = '';
				};

				$scope.querySearch = function(query, myArray) {
					var results = query ? myArray.filter(createFilterFor(query)) : myArray,
						deferred;
					return results;
				};

				function createFilterFor(query) {
					var lowercaseQuery = angular.lowercase(query);
					return function filterFn(item) {
						var lowercaseName = angular.lowercase(item.Name);
						return (lowercaseName.indexOf(lowercaseQuery) === 0);
					};
				};

				if (!$scope.selectedSkillArea && !$scope.selectedSkill && $scope.latestActualInterval === '--:--') {
					$scope.hasMonitorData = false;
				};

				var cancelTimeout = function() {
					if (timeoutPromise) {
						$timeout.cancel(timeoutPromise);
						timeoutPromise = undefined;
					}
				};

				$scope.pollActiveTabDataHelper = function(activeTab) {
					pollActiveTabData(activeTab);
				};

				function pollActiveTabData(activeTab) {

					if ($scope.selectedItem !== null && $scope.selectedItem !== undefined) {
						if (activeTab === 0) {
							loadTraffic();
							$scope.viewObj = intradayTrafficService.getTrafficData();
							getViewObject();
						}
						if (activeTab === 1) {
							loadPerformance();
							$scope.viewObj = intradayPerformanceService.getPerformanceData();
							getViewObject();
						}
						if (activeTab === 2) {
							loadStaffing();
							$scope.viewObj = intradayMonitorStaffingService.getStaffingData();
							getViewObject();
						}
					} else {
						$timeout(function() {
							pollActiveTabData(activeTab);
						}, 1000);
					}
				};

				var loadTraffic = function() {
					if ($scope.selectedItem.Skills) {
						intradayTrafficService.pollSkillAreaData($scope.selectedItem);
					} else {
						intradayTrafficService.pollSkillData($scope.selectedItem);
					}
				};

				var loadStaffing = function() {
					if ($scope.selectedItem.Skills) {
						intradayMonitorStaffingService.pollSkillAreaData($scope.selectedItem);
					} else {
						intradayMonitorStaffingService.pollSkillData($scope.selectedItem);
					}
				};

				var loadPerformance = function() {
					if ($scope.selectedItem.Skills) {
						intradayPerformanceService.pollSkillAreaData($scope.selectedItem);
					} else {
						intradayPerformanceService.pollSkillData($scope.selectedItem);
					}
				};

				var getViewObject = function() {
					$scope.latestActualInterval = $scope.viewObj.latestActualInterval;
					$scope.hasMonitorData = $scope.viewObj.hasMonitorData;

				};

				$scope.$on("$destroy", function(event) {
					cancelTimeout();
				});

				$scope.$on('$locationChangeStart', function() {
					cancelTimeout();
				});

				$scope.configMode = function() {
					$state.go('intraday.config', {
						isNewSkillArea: false
					});
				};

				$scope.toggleModal = function() {
					$scope.DeleteSkillAreaModal = !$scope.DeleteSkillAreaModal;
				};

				$scope.onStateChanged = function(evt, to, params, from) {
					if (to.name !== 'intraday.area')
						return;
					if (params.isNewSkillArea === true) {
						reloadSkillAreas(true);
					} else
						reloadSkillAreas(false);
				};

				$scope.$on('$stateChangeSuccess', $scope.onStateChanged);

				$scope.$on('$viewContentLoaded', function() {
					pollActiveTabData(activeTab);
				});

				var polling = $interval(function() {
					pollActiveTabData(activeTab);
				}, pollingTimeout);

				var notifySkillAreaDeletion = function() {
					var message = $translate.instant('Deleted');
					NoticeService.success(message, 5000, true);
				};

				$scope.$on('$destroy', function() {
					$interval.cancel(polling);
				});

			}
		]);
})();
