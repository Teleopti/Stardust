(function() {

	'use strict';

	angular.module('wfm.outbound')
		.controller('CampaignListGanttCtrl', [
			'$scope',
			'$filter',
			'$q',
			'Toggle',
			'outboundService',
			'miscService',
			'outboundTranslationService',
			'outboundChartService',
			'outboundNotificationService',
			'$sessionStorage',
			campaignListGanttCtrl]);

	function campaignListGanttCtrl($scope, $filter, $q, toggleSvc, outboundService, miscService, outboundTranslationService, outboundChartService, outboundNotificationService, $sessionStorage) {

		$scope.isInitFinished = false;
		$scope.isLoadFinished = false;
		$scope.month = "month";
		$scope.$storage = $sessionStorage;

		$scope.monthpickerOptions = {
			minMode: 'month'
		};

		outboundService.checkPermission($scope).then(function() {
			toggleSvc.togglesLoaded.then(init);
		});

		$scope.viewOneMonthBefore = function() {
			$scope.settings.periodStart = moment($scope.settings.periodStart).add(-1, 'month').toDate();
		};

		$scope.viewOneMonthAfter = function () {
			$scope.settings.periodStart = moment($scope.settings.periodStart).add(1, 'month').toDate();
		};

		function init() {
			$scope.settings = { threshold: null, periodStart: new Date() };
			if ($scope.$storage.visualizationPeriodStart) $scope.settings.periodStart = $scope.$storage.visualizationPeriodStart;
			$scope.isRefreshingGantt = true;
			$scope.ganttOptions = setGanttOptions();
			$scope.timespans = getGanttShadedTimespans();
			$q.all([loadScheduleDataPromise(), renderGanttChartPromise(), getThresholdPromise()]).then(function () {
				updateCampaignStatus(function () {					
					addThresholdChangeWatch();
					addPeriodStartChangeWatch();
					$scope.isRefreshingGantt = false;
				});
			});
			$scope.isInitFinished = true;
		}		

		function refreshGantt() {
			$scope.isRefreshingGantt = true;
			$scope.ganttOptions = setGanttOptions();
			$scope.timespans = getGanttShadedTimespans();
			
			renderGanttChartPromise().then(function () {
				updateCampaignStatus(function () {
					$scope.isRefreshingGantt = false;
				});
			});
		}
		
		function loadExtraScheduleDataPromise() {
			$scope.isRefreshingGantt = true;
			var ganttPeriod = outboundService.getGanttPeriod($scope.settings.periodStart);
			var deferred = $q.defer();
			outboundService.updateCampaignSchedule(ganttPeriod, function () {
				deferred.resolve();
			});
			return deferred.promise;
		}


		function addThresholdChangeWatch() {
			$scope.$watch(function () {
				return $scope.settings.threshold;
			}, function (newValue, oldValue) {
				if (newValue !== oldValue) {
					var thresholdObj = { Value: newValue / 100, Type: 1 };
					outboundService.updateThreshold(thresholdObj, function () {
						updateCampaignStatus(function () {
							$scope.ganttData.forEach(function (dataRow) {
								if (dataRow.expansion) {
									$scope.$broadcast('campaign.chart.refresh', dataRow.campaign);
								}
							});
						});
					});
				}
			});
		}

		function addPeriodStartChangeWatch() {
			$scope.$watch(function () {
				return $scope.settings.periodStart.getFullYear() + $scope.settings.periodStart.getMonth();
			}, function () {
				$scope.$storage.visualizationPeriodStart = $scope.settings.periodStart;

				$q.all([loadExtraScheduleDataPromise(), renderGanttChartPromise()]).then(refreshGantt);	
				
			});
		}


		function getThresholdPromise() {
			var deferred = $q.defer();
			outboundService.getThreshold(function(data) {
				$scope.settings.threshold = data.Value ? Math.round(data.Value * 100) : 0;
				deferred.resolve();
			});
			return deferred.promise;
		}

		function updateCampaignStatus(cb) {
			var ganttPeriod = outboundService.getGanttPeriod($scope.settings.periodStart);
			outboundService.updateCampaignsStatus(ganttPeriod, function success(data) {
				updateAllCampaignGanttDisplay(data);
				$scope.ganttStatistics = data;			
				if (cb) cb();
			});
		}

		function loadScheduleDataPromise() {			
			var deferred = $q.defer();
			var ganttPeriod = outboundService.getGanttPeriod($scope.settings.periodStart);
			outboundService.loadCampaignSchedules(ganttPeriod, function handleSuccess() {
				deferred.resolve();				
			});
			return deferred.promise;
		}
		
		function isLastDayOfGanttPeriod(momentDate) {			
			var ganttPeriod = outboundService.getGanttPeriod($scope.settings.periodStart);
			return momentDate.format("YYYY-MM-DD") == moment(ganttPeriod.PeriodEnd).format("YYYY-MM-DD");
		}

		$scope.headerFormats = {
			month: 'MMMM',
			week: function (column) {				
				return '<div class="week-days-header">'
				+ '<div class="week-start-day">' + (miscService.isLastDayOfMonth(column.date) && isLastDayOfGanttPeriod(column.date) ? '' : column.date.format('D')) + '</div>'
				+ '<div></div>'
				+ '<div class="week-end-day">'  + '</div>'
				+ '</div>';
			}			
		};		

		$scope.tableHeaders = {
			'model.name': ''
		}

		outboundTranslationService.translatePromise('Name').then(function(v) {
			$scope.tableHeaders = {
				'model.name': v
			}
		});

		function readIndex(c) {
			var i = 0;
			for (i; i < $scope.ganttData.length; i++) {
				if ($scope.ganttData[i].id == c.id)
					return i;
			}
			return -1;
		}

		$scope.campaignClicked = function (ev, c) {
			if ($scope.isRefreshingGantt) return;
			if (c.expansion) return;
			if (c.expanded) {
				c.expanded = false;
				$scope.ganttData.splice(readIndex(c) + 1, 1);

			} else {
				c.expanded = true;
				var campaign = { Id: c.id };
				var newDataRow = {
					id: c.id + '_expanded',
					height: '565px',
					expansion: true,
					campaign: campaign,
					isLoadingData: true,
					isRefreshingData: false,
					collapse: function collapseExpansion(ev, index) {
						c.expanded = false;
						$scope.ganttData.splice(index, 1);
					}
				};
				newDataRow.callbacks = {
					ignoreSchedules: getIgnoreScheduleCallback(campaign),
					addManualPlan: getCommandCallback(campaign, newDataRow, $scope),
					removeManualPlan: getCommandCallback(campaign, newDataRow, $scope),
					addManualBacklog: getCommandCallback(campaign, newDataRow, $scope),
					removeManualBacklog: getCommandCallback(campaign, newDataRow, $scope),
					replan: getCommandCallback(campaign, newDataRow, $scope)
				}

				var index = readIndex(c);
				if (index >= 0)
					$scope.ganttData.splice(index + 1, 0, newDataRow);
				getGraphData(campaign, function () {
					newDataRow.isLoadingData = false;
					newDataRow.campaign = campaign;
				});
			}
		};

		function getIgnoreScheduleCallback(campaign) {
			return function(ignoredDates) {
				ignoredDates.forEach(function(ignoredDate) {
					var index = campaign.graphData.dates.indexOf(ignoredDate);
					campaign.graphData.schedules[index] = 0;
					campaign.graphData.unscheduledPlans[index] = campaign.graphData.rawPlans[index] - campaign.graphData.overStaff[index];
				});
				$scope.$broadcast('campaign.chart.clear.selection', campaign);
				$scope.$broadcast('campaign.chart.refresh', campaign);
			};
		}

		function getCommandCallback(campaign, dataRow, scope) {
			return function(resp, done) {
				dataRow.isRefreshingData = true;
				getGraphData(campaign,
					function() {
						scope.$broadcast('campaign.chart.refresh', campaign);
						$scope.$broadcast('campaign.chart.clear.selection', campaign);
						dataRow.isRefreshingData = false;
						outboundNotificationService.notifyCampaignUpdateSuccess(campaign);
						if (done) done();
					});
			};
		}

		function getGraphData(campaign, done) {
			outboundService.getCampaignStatus(campaign.Id, function (campaignStatus) {
				angular.extend(campaign, campaignStatus);
				outboundChartService.getCampaignVisualization(campaign.Id, function (data, translations, manualPlan, closedDays, backlog) {
					campaign.graphData = data;
					campaign.rawManualPlan = manualPlan;
					campaign.isManualBacklog = backlog;
					campaign.translations = translations;
					campaign.closedDays = closedDays;
					updateSingleCampaignGanttDisplay(campaignStatus);
					if (done) done();
				});
			});
		}
		
		function getGanttShadedTimespans() {
			var ganttPeriod = outboundService.getGanttPeriod($scope.settings.periodStart);	
			var weekends = miscService.getAllWeekendsInPeriod(ganttPeriod);
			var timespans = [
				{
					name: "today",
					from: moment().format('YYYY-MM-DD'),
					to: moment().add(1, 'day').format('YYYY-MM-DD'),
					classes: ['gantt-timespan-today']
				}
			];

			angular.forEach(weekends, function (we) {
				var weekend = {
					WeekendStart: moment(we.WeekendStart),
					WeekendEnd: moment(we.WeekendEnd)
				};

				timespans.push({
					from: (weekend.WeekendStart.isSame(moment(ganttPeriod.PeriodStart))) ?
						weekend.WeekendStart.clone().subtract(1, 'day').format('YYYY-MM-DD') :
						weekend.WeekendStart.clone().format('YYYY-MM-DD'),
					to: weekend.WeekendEnd.clone().add(1, 'day').format('YYYY-MM-DD')
				});
			});

			return timespans;
		}
		
		function setGanttOptions() {
			var ganttPeriod = outboundService.getGanttPeriod($scope.settings.periodStart);
			$scope.settings.periodStart = ganttPeriod.PeriodStart;
			return {
				headers: ['month', 'week'],
				fromDate: ganttPeriod.PeriodStart,
				toDate: ganttPeriod.PeriodEnd				
			};
		}

		function renderGanttChartPromise() {
			var ganttPeriod = outboundService.getGanttPeriod($scope.settings.periodStart);
			var deferred = $q.defer();			
			outboundService.getCampaigns(ganttPeriod, function success(data) {
				var ganttArr = [];
				if (data) data.forEach(function (ele, ind) {
					
					var startDate = miscService.getDateFromServer(ele.StartDate);
					var endDate = miscService.getDateFromServer(ele.EndDate);
					var extendedEndDate = moment(endDate).add(1, 'days').toDate();

					ganttArr[ind] = {};
					ganttArr[ind].name = ele.Name;
					ganttArr[ind].id = ele.Id;
					ganttArr[ind].tasks = [];
					ganttArr[ind].tasks[0] = {};
					ganttArr[ind].tasks[0].id = ele.Id;
					ganttArr[ind].tasks[0].from = startDate;
					ganttArr[ind].tasks[0].to = extendedEndDate;
					ganttArr[ind].tasks[0].color = 'rgba(0,0,0,0.6)';
					ganttArr[ind].tasks[0].tooltips = {
						'enabled': true
					};
					ganttArr[ind].classes = [];
					ganttArr[ind].tasks[0].campaignName = ele.Name;					
				});
				$scope.ganttData = ganttArr;
				$scope.isLoadFinished = true;

				deferred.resolve();
			});

			return deferred.promise;
		}

		function updateGanttRowFromCampaignStatus(row, campaignStatus) {
			row.campaignNameClass = null;
			row.tasks[0].color = campaignStatus.IsScheduled ? '#C2E085' : '#ffc36b';
			campaignStatus.WarningInfo.forEach(function (warning) {
				if (warning.TypeOfRule == 'CampaignUnderSLA') {
					row.campaignNameClass = 'campaign-late';
				}
				if (warning.TypeOfRule == 'CampaignOverstaff') {
					row.campaignNameClass = 'campaign-early';
				}
			});
		}

		function updateWarningInfo(row, campaignSummary) {
			row.ClassesEnhancedHeader = 'anti-enhanced-gantt-header';
			row.campaign.WarningInfo = campaignSummary.WarningInfo;
		}

		function updateAllCampaignGanttDisplay(campaignStatusList) {
			campaignStatusList.forEach(function (campaignStatus) {
				updateSingleCampaignGanttDisplay(campaignStatus);
			});
		}

		function updateSingleCampaignGanttDisplay(campaignStatus) {
			$scope.ganttData.forEach(function (dataRow, indx) {
				if (campaignStatus.CampaignSummary.Id == dataRow.id) {
					return updateGanttRowFromCampaignStatus(dataRow, campaignStatus);
				}
				if (campaignStatus.CampaignSummary.Id + '_expanded' == dataRow.id) {
					return updateWarningInfo(dataRow, campaignStatus);
				}
			});
		}

		return {
			init: init,
			updateAllCampaignGanttDisplay:updateAllCampaignGanttDisplay
	}
	}

})();