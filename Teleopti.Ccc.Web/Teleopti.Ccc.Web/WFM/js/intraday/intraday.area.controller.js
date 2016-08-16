(function () {
	'use strict';
	angular.module('wfm.intraday')
	.controller('IntradayAreaCtrl', [
		'$scope', '$state', 'intradayService', '$filter', 'NoticeService', '$timeout', '$compile', '$translate', 'intradaySkillService', 'intradayTrafficService', 'intradayPerformanceService',
		function ($scope, $state, intradayService, $filter, NoticeService, $timeout, $compile, $translate, intradaySkillService, intradayTrafficService, intradayPerformanceService) {

			var autocompleteSkill;
			var autocompleteSkillArea;
			var timeoutPromise;
			var interval;
			var pollingTimeout = 60000;
			$scope.DeleteSkillAreaModal = false;
			$scope.hiddenArray = [];
			$scope.prevArea;
			$scope.drillable;
			var message = $translate.instant('WFMReleaseNotificationWithoutOldModuleLink')
			.replace('{0}', $translate.instant('Intraday'))
			.replace('{1}', "<a href=' http://www.teleopti.com/wfm/customer-feedback.aspx' target='_blank'>")
			.replace('{2}', '</a>');
			var prevSkill = {};
			$scope.currentInterval = [];
			$scope.chart;
			$scope.trafficData = {};
			$scope.performanceData = {};
			$scope.format = intradayService.formatDateTime;

			NoticeService.info(message, null, true);

			var getAutoCompleteControls = function () {
				var autocompleteSkillDOM = document.querySelector('.autocomplete-skill');
				autocompleteSkill = angular.element(autocompleteSkillDOM).scope();

				var autocompleteSkillAreaDOM = document.querySelector('.autocomplete-skillarea');
				autocompleteSkillArea = angular.element(autocompleteSkillAreaDOM).scope();
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

			$scope.skillSelected = function (item) {
				$scope.selectedItem = item;
				clearSkillAreaSelection();
				pollSkillMonitorData();
			};

			$scope.skillAreaSelected = function (item) {
				$scope.selectedItem = item;
				intradaySkillService.setSkill()
				clearSkillSelection();
				pollSkillAreaMonitorData();
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

				var setResult = function (result) {
					if (!result.LatestActualIntervalEnd) {
						$scope.latestActualInterval = '--:--';
						$scope.HasMonitorData = false;
						return;
					} else {
						$scope.latestActualInterval = $filter('date')(result.LatestActualIntervalStart, 'shortTime') + ' - ' + $filter('date')(result.LatestActualIntervalEnd, 'shortTime');

						$scope.timeSeries = [];
						angular.forEach(result.DataSeries.Time, function (value, key) {
							this.push($filter('date')(value, 'shortTime'));
						}, $scope.timeSeries);
						$scope.timeSeries.splice(0, 0, 'x');
						$scope.currentInterval.splice(0, 0, 'Current');

						$scope.HasMonitorData = true;

						$scope.trafficData = intradayTrafficService.setTrafficData(result);
						$scope.performanceData = intradayPerformanceService.setPerformanceData(result);
						loadTrafficChart();
						loadPerformenceChart();
					}
				};

				if (!$scope.selectedSkillArea && !$scope.selectedSkill && $scope.latestActualInterval === '--:--') {
					$scope.HasMonitorData = false;
				}

				function findCurrent(currentMaxValue) {
					for (var i = 0; i < $scope.timeSeries.length; i++) {
						if ($scope.timeSeries[i] === $scope.latestActualInterval.split(' - ')[0]) {
							$scope.currentInterval[i] = currentMaxValue;
						} else {
							if (i > 0) {
								$scope.currentInterval[i] = null;
							}
						}
					}
				}

				var getMaxValueFromVisibleDataSeries = function (dataPrioListY1, dataPrioListY2) {
					var maxValue = 0;
					$scope.chartCurrentYAxis = 'y1';
					angular.forEach(dataPrioListY1, function (value, key) {
						if ($scope.chartHiddenLines.indexOf(value.Series[0]) === -1) {
							if (value.Max > maxValue)
							maxValue = value.Max;
						}
					});
					if (maxValue === 0) {
						$scope.chartCurrentYAxis = 'y2';
						angular.forEach(dataPrioListY2, function (value, key) {
							if ($scope.chartHiddenLines.indexOf(value.Series[0]) === -1) {
								if (value.Max > maxValue)
								maxValue = value.Max;
							}
						});
					}
					return maxValue * 1.1;
				};

				$scope.resizeChart = function () {
					$timeout(function () {
						$scope.chart.resize();
						$scope.prefChart.resize();
					}, 1000);
				}

				var loadTrafficChart = function () {
					$scope.chartHiddenLines = $scope.hiddenArray;
					var max = getMaxValueFromVisibleDataSeries([
						$scope.trafficData.forecastedCallsObj,
						$scope.trafficData.actualCallsObj
					], [
						$scope.trafficData.forecastedAverageHandleTimeObj,
						$scope.trafficData.actualAverageHandleTimeObj
					]);

					findCurrent(max);
					var intervalsList = [];
					for (interval = 0; interval < $scope.timeSeries.length - 1; interval += 4) {
						intervalsList.push(interval);
					}
					$scope.chart = c3.generate({
						bindto: '#intradayChart',
						data: {
							x: 'x',
							columns: [
								$scope.timeSeries,
								$scope.trafficData.forecastedCallsObj.Series,
								$scope.trafficData.actualCallsObj.Series,
								$scope.trafficData.forecastedAverageHandleTimeObj.Series,
								$scope.trafficData.actualAverageHandleTimeObj.Series,
								$scope.currentInterval
							],
							hide: $scope.chartHiddenLines,
							colors: {
								Forecasted_calls: '#9CCC65',
								Calls: '#4DB6AC',
								Forecasted_AHT: '#F06292',
								AHT: '#BA68C8',
								Current: '#cacaca'
							},
							type: 'line',
							types: {
								Current: 'bar'
							},
							names: {
								Forecasted_calls: $translate.instant('ForecastedCalls') + ' ←',
								Calls: $translate.instant('Calls') + ' ←',
								Forecasted_AHT: $translate.instant('ForecastedAverageHandleTime') + ' →',
								AHT: $translate.instant('AverageHandlingTime') + ' →',
								Current: $translate.instant('latestActualInterval')
							},
							axes: {
								AHT: 'y2',
								Forecasted_AHT: 'y2',
								Current: $scope.chartCurrentYAxis
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
									loadTrafficChart();
								}
							}
						}
					});
				}

				var loadPerformenceChart = function () {
					$scope.chartHiddenLines = $scope.hiddenArray;

					var max = getMaxValueFromVisibleDataSeries([
						$scope.performanceData.serviceLevelObj,
						$scope.performanceData.abandonedRateObj
					], [
						$scope.performanceData.averageSpeedOfAnswerObj
					]);

					findCurrent(max);
					var intervalsList = [];
					for (interval = 0; interval < $scope.timeSeries.length - 1; interval += 4) {
						intervalsList.push(interval);
					}
					$scope.prefChart = c3.generate({
						bindto: '#perfChart',
						data: {
							x: 'x',
							columns: [
								$scope.performanceData.serviceLevelObj.Series,
								$scope.performanceData.abandonedRateObj.Series,
								$scope.performanceData.averageSpeedOfAnswerObj.Series,
								$scope.timeSeries,
								$scope.currentInterval

							],
							hide: $scope.chartHiddenLines,
							colors: {
								Service_level: '#F06292',
								Abandoned_rate: '#4DB6AC',
								ASA: '#9CCC65',
								Current: '#cacaca'
							},
							type: 'line',
							types: {
								Current: 'bar'
							},
							names: {
								Service_level: $translate.instant('ServiceLevelPercentSign') + ' ←',
								Abandoned_rate: $translate.instant('AbandonedRate') + ' ←',
								ASA: $translate.instant('AverageSpeedOfAnswer') + ' →',
								Current: $translate.instant('latestActualInterval')
							},
							axes: {
								ASA: 'y2',
								Current: $scope.chartCurrentYAxis
							}
						},
						axis: {
							y2: {
								show: true,
								label: $translate.instant('Seconds'),
								tick: {
									format: d3.format('.1f')
								}
							},
							y: {
								label: '%',
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
									loadPerformenceChart();
								}
							}
						}
					});
				}

				var cancelTimeout = function () {
					if (timeoutPromise) {
						$timeout.cancel(timeoutPromise);
						timeoutPromise = undefined;
					}
				};

				var pollSkillMonitorData = function (chartTypeSelcted) {
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

						$scope.$on("$destroy", function (event) {
							cancelTimeout();
						});

						$scope.$on('$locationChangeStart', function () {
							cancelTimeout();
						});

						$scope.configMode = function () {
							$state.go('intraday.config', { isNewSkillArea: false });
						};

						$scope.toggleModal = function () {
							$scope.DeleteSkillAreaModal = !$scope.DeleteSkillAreaModal;
						};

						$scope.onStateChanged = function (evt, to, params, from) {
							if (to.name !== 'intraday.area')
							return;

							if (params.isNewSkillArea === true)
							reloadSkillAreas(true);
							else
							reloadSkillAreas(false);
						};

						$scope.$on('$stateChangeSuccess', $scope.onStateChanged);

						var notifySkillAreaDeletion = function () {
							var message = $translate.instant('Deleted');
							NoticeService.success(message, 5000, true);
						};
					}
				]);
			})();
