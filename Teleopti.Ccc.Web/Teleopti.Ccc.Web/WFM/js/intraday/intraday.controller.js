(function () {
	'use strict';
	angular.module('wfm.intraday')
	.controller('IntradayCtrl', [
		'$scope', '$state', 'intradayService', '$filter', 'NoticeService', '$timeout', '$compile',
		function ($scope, $state, intradayService, $filter, NoticeService, $timeout, $compile) {

			var autocompleteSkill;
			var autocompleteSkillArea;
			var timeoutPromise;
			var pollingTimeout = 60000;
			$scope.DeleteSkillAreaModal = false;
			$scope.showIncoming = true;
			$scope.showStaffing = false;
			$scope.showProductivity = false;
			$scope.showSummary = false;
			$scope.selectedIndex = 0;

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

				$scope.toggleModal = function() {
				$scope.DeleteSkillAreaModal = !$scope.DeleteSkillAreaModal;
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
					NoticeService.success("Deleted Area ", 5000, true);
				};

				$scope.querySearch = function (query, myArray) {
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

				$scope.selectedSkillChange = function (item) {
					if (this.selectedSkill) {
						$scope.skillSelected(item);
					}
				};

				$scope.skillSelected = function (item) {
					$scope.selectedItem = item;
					clearSkillAreaSelection();
					pollSkillMonitorData();
				};

				$scope.openSkillFromArea = function (item) {
					autocompleteSkill.selectedSkill = item;
					$scope.skillSelected(item);
				};

			    var setResult = function(result) {
					if (!result.LatestStatsTime) {
						$scope.latestStatsTime = '--:--';
						$scope.HasMonitorData = false;
						return;
					}
					$scope.latestStatsTime = $filter('date')(result.LatestStatsTime, 'shortTime');
					$scope.forecastedCalls = $filter('number')(result.Summary.ForecastedCalls, 1);
					$scope.forecastedAverageHandleTime = $filter('number')(result.Summary.ForecastedAverageHandleTime, 1);
					$scope.offeredCalls = $filter('number')(result.Summary.OfferedCalls, 1);
					$scope.averageHandleTime = $filter('number')(result.Summary.AverageHandleTime, 1);
					$scope.timeSeries = result.DataSeries.Time;
					$scope.forecastedCallsSeries = result.DataSeries.ForecastedCalls;
					$scope.actualCallsSeries = result.DataSeries.OfferedCalls;
					$scope.forecastedAverageHandleTimeSeries = result.DataSeries.ForecastedAverageHandleTime;
					$scope.actualAverageHandleTimeSeries = result.DataSeries.AverageHandleTime;
					$scope.forecastActualCallsDifference = $filter('number')(result.Summary.ForecastedActualCallsDiff, 1);
					$scope.forecastActualAverageHandleTimeDifference = $filter('number')(result.Summary.ForecastedActualHandleTimeDiff, 1);

					$scope.HasMonitorData = true;
				};

				var pollSkillMonitorData = function () {
					cancelTimeout();
					intradayService.getSkillMonitorData.query(
						{
							id: $scope.selectedItem.Id
						})
						.$promise.then(function (result) {
							timeoutPromise = $timeout(pollSkillMonitorData, pollingTimeout);
							setResult(result);
							loadIntradayChart();
						},
						function (error) {
							timeoutPromise = $timeout(pollSkillMonitorData, pollingTimeout);
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
								setResult(result);
								loadIntradayChart();
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

				var deselectAll = function(){
							$scope.showIncoming = false;
							$scope.showStaffing = false
							$scope.showProductivity = false;
						}
				$scope.toggleOthers = function(index, intraCard){
							deselectAll();
							$scope.selectedIndex = index;
							if (intraCard === 'incoming') {
								$scope.showIncoming = true;
					} else if (intraCard === 'staffing') {
								$scope.showStaffing = true;
							}
							else if (intraCard === 'productivity') {
								$scope.showProductivity = true;
							}
						}

						var loadIntradayChart = function() {
							$scope.chartForecastedCalls = $scope.forecastedCallsSeries;

							$scope.chartForecastedCalls.splice(0,0,"Forecasted_calls");
							$scope.actualCallsSeries.splice(0,0,"Actual_calls");
							$scope.forecastedAverageHandleTimeSeries.splice(0,0,"Forecasted_AHT");
							$scope.actualAverageHandleTimeSeries.splice(0,0,"AHT");
							c3.generate({
								bindto: '#myChart',
								data: {
									columns: [
										$scope.chartForecastedCalls,
										$scope.actualCallsSeries,
										$scope.forecastedAverageHandleTimeSeries,
										$scope.actualAverageHandleTimeSeries
									],
									colors: {
            				Forecasted_calls: 'blue',
            				Actual_calls: 'cyan',
            				Forecasted_AHT: 'tomato',
										AHT: 'red',
        					},
									axes: {
										AHT: 'y2',
										Forecasted_AHT: 'y2'
									}
								},
								axis: {
									y2: {
										show: true,
										label: 'AHT'
									},
									y:{
										label: 'Calls'
									},
									x: {
										//type: 'category',
										//categories: $scope.timeSeries,
										label: 'interval'
									}
								}
							});
						}

					}
				]);
			})();
