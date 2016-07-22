(function() {
	'use strict';
	angular.module('wfm.intraday')
		.controller('IntradayStaffingCtrl', [
			'$scope', '$stateParams', 'intradayStaffingService', 'Toggle', 'intradayService', '$filter', 'NoticeService', '$translate', '$q',
			function($scope, $stateParams, intradayStaffingService, toggleService, intradayService, $filter, NoticeService, $translate, $q) {
				var chartData = {};
				$scope.isVisible = true;
				$scope.skills;
				$scope.selectedSkill = null;
				$scope.viewCal = true;

				if ($stateParams.date) {
					$scope.intervalDate = $stateParams.date;
				} else {
					$scope.intervalDate = moment();
				};
				if ($stateParams.id) {
					$scope.selectedSkill = {
						Name: "Loading"
					};
					intradayStaffingService.matchSkill($stateParams.id).then(function(response) {
						$scope.selectedSkill = response;
					});
				};

				$scope.$watchGroup(['intervalDate', 'selectedSkill','selectedSkillArea'], function(newValues, oldValues, scope) {
					if (newValues[1] || newValues[2]) {
						$scope.runIntradayFetch()
					}

				});

				toggleService.togglesLoaded.then(function(result) {
					$scope.isVisible = toggleService.Intraday_ResourceCalculateReadModel_39200;
				});

				intradayService.getSkills.query().$promise.then(function(response) {
					$scope.skills = response;
				});

				intradayService.getSkillAreas.query().$promise.then(function(response) {
					$scope.skillAreas = response.SkillAreas;
				});

				$scope.querySearch = function(query) {
					var results = query ? $scope.skills.filter(matchQuery(query)) : $scope.skills,
						deferred;
					return results;
				};

				var matchQuery = function(query) {
					var lowercaseQuery = angular.lowercase(query);
					return function filterFn(item) {
						var lowercaseName = angular.lowercase(item.Name);
						return (lowercaseName.indexOf(lowercaseQuery) === 0);
					};
				};

				$scope.TriggerResourceCalculate = function() {
					intradayStaffingService.TriggerResourceCalculate.query().$promise.then(function(response) {})
				};

				$scope.updateSelectedSkill = function(skill) {
					if (skill) {
						$scope.skillIsSelected = true
						$scope.selectedSkill = skill;
					}else {
						$scope.skillIsSelected = false;
						$scope.selectedSkill = null;
					}
				};

				$scope.updateSelectedArea = function(area) {
					if (area) {
						$scope.selectedSkillArea = area;
						$scope.areaIsSelected = true;
					} else {
						$scope.areaIsSelected = false;
						$scope.selectedSkillArea = null;
					}
				};

				var initArrays = function() {
					chartData.Forcast = ['Forecasted'];
					chartData.Staffing = ['Staffing'];
					chartData.Intervals = ['x'];
				};
				var extractRelevantData = function(data) {
					data.forEach(function(single) {
						chartData.Forcast.push(single.Forecast.toFixed(2));
						chartData.Staffing.push(single.StaffingLevel.toFixed(2));
						chartData.Intervals.push(new Date(single.StartDateTime));
					});
					generateChart();
				};

				var fetchDataForSkill = function(date,id){
					intradayStaffingService.resourceCalculate.query({
						date: date,
						skillId: id
					}).$promise.then(function(response) {
						extractRelevantData(response);
						$scope.loading = false;
					});
				};
				var fetchDataForArea = function(date,id){
					intradayStaffingService.resourceCalculateForArea.query({
						date: date,
						skillAreaId: id
					}).$promise.then(function(response) {
						extractRelevantData(response);
						$scope.loading = false;
					});
				}

				$scope.runIntradayFetch = function() {
					initArrays();
					var actualDate = moment($scope.intervalDate).format('YYYY-MM-DD');
					if (($scope.selectedSkill === null || $scope.selectedSkill === undefined) && ($scope.selectedSkillArea === null || $scope.selectedSkillArea === undefined))
						return;
					var actualId = $scope.skillIsSelected ? $scope.selectedSkill.Id : $scope.selectedSkillArea.Id;
					$scope.loading = true;
					if (actualId) {
						if ($scope.skillIsSelected) {
							fetchDataForSkill(actualDate, actualId)
						}else {
							fetchDataForArea(actualDate, actualId)
						}
					}
				};

				$scope.regenerateChart = function() {
					$scope.viewCal = !$scope.viewCal
				}

				initArrays();
				var generateChart = function() {
					c3.generate({
						bindto: '#staffingChart',
						data: {
							x: 'x',
							columns: [
								chartData.Intervals,
								chartData.Forcast,
								chartData.Staffing,
							],
							selection: {
								enabled: true,
							},
							types: {
								Forecasted: 'bar'
							},
						},
						axis: {
							x: {
								localtime: false,
								type: 'timeseries',
								tick: {
									format: '%H:%M'
								}
							}
						},
						zoom: {
							enabled: true,
						},
					});
				};
				generateChart();

			}
		]);
})();
