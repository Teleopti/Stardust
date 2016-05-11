(function () {
	'use strict';
	angular.module('wfm.intraday')
	.controller('IntradayAreaCtrl', [
		'$scope', '$state', 'intradayService', '$filter', 'NoticeService', '$timeout', '$compile', '$translate',
		function ($scope, $state, intradayService, $filter, NoticeService, $timeout, $compile, $translate) {

			var autocompleteSkill;
			var autocompleteSkillArea;
			var timeoutPromise;
			var interval;
			var pollingTimeout = 60000;
			$scope.DeleteSkillAreaModal = false;
			$scope.showIncoming = true;
			$scope.showStaffing = false;
			$scope.showProductivity = false;
			$scope.showSummary = false;
			$scope.selectedIndex = 0;
			$scope.hiddenArray = [];
			$scope.prevArea;
			$scope.drillable;
			var message = "Intraday has been improved! We appreciate your <a href='http://www.teleopti.com/wfm/customer-feedback.aspx' target='_blank'>feedback.</a>";
			var prevSkill = {};
			$scope.currentInterval = [];

			NoticeService.info(message, null, true);

			var getAutoCompleteControls = function () {
				var autocompleteSkillDOM = document.querySelector('.autocomplete-skill');
				autocompleteSkill = angular.element(autocompleteSkillDOM).scope();

				var autocompleteSkillAreaDOM = document.querySelector('.autocomplete-skillarea');
				autocompleteSkillArea = angular.element(autocompleteSkillAreaDOM).scope();
			};

			$scope.format = intradayService.formatDateTime;

			$scope.onStateChanged = function (evt, to, params, from) {
				if (to.name !== 'intraday.area')
				return;

				if (params.isNewSkillArea === true)
				reloadSkillAreas(true);
				else
				reloadSkillAreas(false);
			};

			$scope.$on('$stateChangeSuccess', $scope.onStateChanged);

			var reloadSkillAreas = function (isNew) {
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

			$scope.configMode = function () {
				$state.go('intraday.config', { isNewSkillArea: false });
			};

			$scope.toggleModal = function () {
				$scope.DeleteSkillAreaModal = !$scope.DeleteSkillAreaModal;
			};

			$scope.deleteSkillArea = function (skillArea) {
				cancelTimeout();
				intradayService.deleteSkillArea.remove(
					{
						id: skillArea.Id
					})
					.$promise.then(function (result) {
						$scope.skillAreas.splice($scope.skillAreas.indexOf(skillArea), 1);
						$scope.selectedItem = null;
						$scope.HasMonitorData = false;
						clearSkillAreaSelection();
						notifySkillAreaDeletion();
					});

					$scope.toggleModal();
				};

				var notifySkillAreaDeletion = function () {
					var message = $translate.instant('Deleted');
					NoticeService.success(message, 5000, true);
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

				$scope.openSkillFromArea = function (item) {
					prevSkill = item;
					autocompleteSkill.selectedSkill = item;
					$scope.drillable = true;
				};

				$scope.openSkillAreaFromSkill = function () {
					autocompleteSkillArea.selectedSkillArea = $scope.prevArea;
					$scope.drillable = false;
				};

				$scope.findCurrent = function () {
					for (var i = 0; i < $scope.timeSeries.length; i++) {
						if ($scope.timeSeries[i] === $scope.latestActualInterval){
							$scope.currentInterval[i] = 300;
						}else{
							if (i > 0) {
								$scope.currentInterval[i] = null;
							}
						}
					}
				}

				var setResult = function (result) {
					if (!result.LatestActualInterval) {
						$scope.latestActualInterval = '--:--';
						$scope.HasMonitorData = false;
						return;
					} else {
						$scope.latestActualInterval = $filter('date')(result.LatestActualInterval, 'shortTime');
						$scope.forecastedCalls = $filter('number')(result.Summary.ForecastedCalls, 1);
						$scope.forecastedAverageHandleTime = $filter('number')(result.Summary.ForecastedAverageHandleTime, 1);
						$scope.offeredCalls = $filter('number')(result.Summary.OfferedCalls, 1);
						$scope.averageHandleTime = $filter('number')(result.Summary.AverageHandleTime, 1);
						$scope.timeSeries = [];
						angular.forEach(result.DataSeries.Time, function (value, key) {
							this.push($filter('date')(value, 'shortTime'));
						}, $scope.timeSeries)
						$scope.forecastedCallsSeries = result.DataSeries.ForecastedCalls;
						$scope.actualCallsSeries = result.DataSeries.OfferedCalls;
						$scope.forecastedAverageHandleTimeSeries = result.DataSeries.ForecastedAverageHandleTime;
						$scope.actualAverageHandleTimeSeries = result.DataSeries.AverageHandleTime;
						$scope.forecastActualCallsDifference = $filter('number')(result.Summary.ForecastedActualCallsDiff, 1);
						$scope.forecastActualAverageHandleTimeDifference = $filter('number')(result.Summary.ForecastedActualHandleTimeDiff, 1);

						$scope.timeSeries.splice(0, 0, 'x');
						$scope.currentInterval.splice(0, 0, 'Current');
						$scope.forecastedCallsSeries.splice(0, 0, 'Forecasted_calls');
						$scope.actualCallsSeries.splice(0, 0, 'Actual_calls');
						$scope.forecastedAverageHandleTimeSeries.splice(0, 0, 'Forecasted_AHT');
						$scope.actualAverageHandleTimeSeries.splice(0, 0, 'AHT');
						$scope.HasMonitorData = true;
						loadIntradayChart();
					}
				};

				var clearPrev = function () {
					$scope.drillable = false;
					$scope.prevArea = false;
					prevSkill = false;
				}

				$scope.selectedSkillChange = function (item) {
					if (item) {
						$scope.skillSelected(item);
						$scope.hiddenArray = [];

						if (!(prevSkill === autocompleteSkill.selectedSkill)) {
							clearPrev();
						}
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
							if (timeoutPromise)
							return;
							timeoutPromise = $timeout(pollSkillMonitorData, pollingTimeout);
							setResult(result);
						},
						function (error) {
							timeoutPromise = $timeout(pollSkillMonitorData, pollingTimeout);
							$scope.HasMonitorData = false;
						});
					};

					$scope.selectedSkillAreaChange = function (item) {
						if (item) {
							$scope.skillAreaSelected(item);
							$scope.hiddenArray = [];
							$scope.prevArea = autocompleteSkillArea.selectedSkillArea;
						}
						if ($scope.drillable === true && autocompleteSkillArea.selectedSkillArea) {
							$scope.drillable = false;
						}
					};

					$scope.skillAreaSelected = function (item) {
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
								if (timeoutPromise)
								return;
								timeoutPromise = $timeout(pollSkillAreaMonitorData, pollingTimeout);
								setResult(result);
							},
							function (error) {
								timeoutPromise = $timeout(pollSkillAreaMonitorData, pollingTimeout);
								$scope.HasMonitorData = false;
							});
						}

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

						$scope.$on("$destroy", function (event) {
							cancelTimeout();
						});

						$scope.$on('$locationChangeStart', function () {
							cancelTimeout();
						});

						var cancelTimeout = function () {
							if (timeoutPromise) {
								$timeout.cancel(timeoutPromise);
								timeoutPromise = undefined;
							}
						};

						var deselectAll = function () {
							$scope.showIncoming = false;
							$scope.showStaffing = false;
							$scope.showProductivity = false;
						}
						$scope.toggleOthers = function (index, intraCard) {
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

						if (!$scope.selectedSkillArea && !$scope.selectedSkill && $scope.latestActualInterval === '--:--') {
							$scope.HasMonitorData = false;
						}

						var loadIntradayChart = function () {
							$scope.chartHiddenLines = $scope.hiddenArray;
							$scope.findCurrent();
							var intervalsList = [];
							for (interval = 0; interval < $scope.timeSeries.length - 1; interval += 4) {
								intervalsList.push(interval);
							}
							c3.generate({
								bindto: '#intradayChart',
								data: {
									x: 'x',
									columns: [
										$scope.timeSeries,
										$scope.forecastedCallsSeries,
										$scope.actualCallsSeries,
										$scope.forecastedAverageHandleTimeSeries,
										$scope.actualAverageHandleTimeSeries,
										$scope.currentInterval
									],
									hide: $scope.chartHiddenLines,
									colors: {
										Forecasted_calls: '#9CCC65',
										Actual_calls: '#4DB6AC',
										Forecasted_AHT: '#F06292',
										AHT: '#BA68C8',
										Current:'#cacaca',
									},
									type: 'line',
									types: {
										Current: 'bar',
									},
									names: {
										Forecasted_calls: $translate.instant('ForecastedCalls'),
										Actual_calls: $translate.instant('Calls'),
										Forecasted_AHT: $translate.instant('ForecastedAverageHandleTime'),
										AHT: $translate.instant('AverageHandlingTime'),
										Current:$translate.instant('latestActualInterval'),
									},
									axes: {
										AHT: 'y2',
										Forecasted_AHT: 'y2',
									}
								},
								axis: {
									y2: {
										show: true,
										label: 'AHT',
										tick: {
											format: d3.format('.1f')
										}
									},
									y: {
										label: $translate.instant('Calls'),
										tick: {
											format: d3.format('.1f')
										}
									},
									x: {
										label: $translate.instant('SkillTypeTime'),
										type: 'category',
										tick: {
											fit: true,
											centered: true,
											multiline: false,
											values: intervalsList
										},
										categories: $scope.timeSeries
									}
								},
								legend: {
									item: {
										onclick: function (id) {
											if ($scope.chartHiddenLines.indexOf(id) > -1) {
												$scope.chartHiddenLines.splice($scope.chartHiddenLines.indexOf(id), 1);
											} else {
												$scope.chartHiddenLines.push(id);
											}
											loadIntradayChart();
										}
									}
								}
							});
						}

					}
				]);
			})();
