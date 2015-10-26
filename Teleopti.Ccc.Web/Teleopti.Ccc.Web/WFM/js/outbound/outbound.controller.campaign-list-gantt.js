(function() {

	'use strict';

	angular.module('wfm.outbound')
		.controller('CampaignListGanttCtrl', [
			'$scope',
			'$filter',
			'$q',
			'OutboundToggles',
			'outboundService',
			'miscService',
			'outboundTranslationService',
			'outboundChartService',
			'outboundNotificationService',
			campaignListGanttCtrl]);

	function campaignListGanttCtrl($scope, $filter, $q, OutboundToggles, outboundService, miscService, outboundTranslationService, outboundChartService, outboundNotificationService) {

		$scope.isGanttEnabled = false;
		$scope.isLoadFinished = false;
		$scope.isNavigationEnabled = false;
		$scope.month = "month";

		$scope.$watch(function() {
			return OutboundToggles.ready;
		}, function(value) {
			if (value) {
				if (OutboundToggles.isGanttEnabled()) {
					$scope.isGanttEnabled = true;
					init();
				}
				if (OutboundToggles.isNavigationEnabled()) $scope.isNavigationEnabled = true;
			}
		});		

		$scope.settings = { threshold: null };

		$scope.onChangeGanttDate = function () {
			$q.all([setVisualizationPeriod(), setDefaultPeriod()]).then(function () {
				refreshGantt();
			});
			};

		$scope.viewOneMonthBefore = function () {
			$q.all([visualizationPeriodPlusOneMonth(), defaultPeriodPlusOneMonth()]).then(function () {
				refreshGantt();
			});
		}

		$scope.viewOneMonthAfter = function () {
			$q.all([visualizationPeriodMinusOneMonth(), defaultPeriodMinusOneMonth()]).then(function() {
				refreshGantt();
			});
		}

		function defaultPeriodMinusOneMonth() {
			var deferred = $q.defer();
			outboundService.defaultPeriodMinusOneMonth(function () {
				deferred.resolve();
			});
			return deferred.promise;
		}

		function visualizationPeriodMinusOneMonth() {
			var deferred = $q.defer();
			outboundService.visualizationPeriodMinusOneMonth(function () {
				deferred.resolve();
			});
			return deferred.promise;
		}

		function defaultPeriodPlusOneMonth() {
			var deferred = $q.defer();
			outboundService.defaultPeriodPlusOneMonth(function () {
				deferred.resolve();
			});
			return deferred.promise;
		}

		function visualizationPeriodPlusOneMonth() {
			var deferred = $q.defer();
			outboundService.visualizationPeriodPlusOneMonth(function() {
				deferred.resolve();
			});
			return deferred.promise;
		}

		function setDefaultPeriod() {
			var deferred = $q.defer();
			outboundService.setDefaultPeriod($scope.settings.ganttStartDate,function() {
				deferred.resolve();
			});
			return deferred.promise;
		}

		function setVisualizationPeriod() {
			var deferred = $q.defer();
			outboundService.setVisualizationPeriod($scope.settings.ganttStartDate,function() {
				deferred.resolve();
			});
			return deferred.promise;
		}

		function init() {
			$scope.isRefreshingGantt = true;
			$scope.ganttOptions = setGanttOptions();
			$q.all([getGanttVisualization(), loadWithinPeriod(), getThreshold()]).then(function() {
				getListCampaignsWithinPeriod(function() {
					$scope.isRefreshingGantt = false;
					addThresholdChangeWatch();
				});
			});
		}

		function refreshGantt() {
			$scope.isRefreshingGantt = true;
			$scope.ganttOptions = setGanttOptions();
			$q.all([getGanttVisualization()]).then(function () {
				getListCampaignsWithinPeriod(function () {
					$scope.isRefreshingGantt = false;
				});
			});
		}

		function addThresholdChangeWatch() {
			$scope.$watch(function () {
				return $scope.settings.threshold;
			}, function (newValue, oldValue) {
				if (newValue !== oldValue) {
					var thresholdObj = { Value: newValue / 100, Type: 1 };
					outboundService.updateThreshold(thresholdObj, function (data) {
						getListCampaignsWithinPeriod(function () {
							$scope.ganttData.forEach(function (dataRow, indx) {
								if (dataRow.expansion) {
									$scope.$broadcast('campaign.chart.refresh', dataRow.campaign);
								}
							});
						});
					});
				}
			});
		}

		function getThreshold() {
			var deferred = $q.defer();
			outboundService.getThreshold(function(data) {
				$scope.settings.threshold = data.Value ? Math.round(data.Value * 100) : 0;
				deferred.resolve();
			});
			return deferred.promise;
		}

		function getListCampaignsWithinPeriod(cb) {
			outboundService.listCampaignsWithinPeriod(function success(data) {
				updateAllCampaignGanttDisplay(data);
				$scope.ganttStatistics = data;			
				if (cb) cb();
			});
		}

		function loadWithinPeriod() {			
			var deferred = $q.defer();
			outboundService.loadWithinPeriod(function handleSuccess() {
				deferred.resolve();				
			});
			return deferred.promise;
		}
		
		$scope.headerFormats = {
			month: 'MMMM',
			week: function (column) {				
				return '<div class="week-days-header">'
				+ '<div class="week-start-day">' + (miscService.isLastDayOfMonth(column.date) ? '' : column.date.format('D')) + '</div>'
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

		function getCommandCallback(campaign, dataRow, scope) {			
			return function (resp, done) {
				dataRow.isRefreshingData = true;
				getGraphData(campaign, function () {
					scope.$broadcast('campaign.chart.refresh', campaign);
					$scope.$broadcast('campaign.chart.clear.selection', campaign);
					dataRow.isRefreshingData = false;
					outboundNotificationService.notifyCampaignUpdateSuccess(campaign);
					if (done) done();
				});							
			}			
		}

		function getGraphData(campaign, done) {		
			outboundService.getCampaignDetail(campaign.Id, function (_campaign) {
				angular.extend(campaign, _campaign);
				outboundChartService.getCampaignVisualization(campaign.Id, function (data, translations, manualPlan, closedDays, backlog) {
					campaign.graphData = data;
					campaign.rawManualPlan = manualPlan;
					campaign.isManualBacklog = backlog;
					campaign.translations = translations;
					campaign.closedDays = closedDays;
					updateSingleCampaignGanttDisplay(_campaign);
					if (done) done();					
				});
			});
		}
		
		$scope.timespans = [
			{
				name: "today", 
				from: moment().format('YYYY-MM-DD'), 
				to: moment().add(1, 'day').format('YYYY-MM-DD'),
				classes: ['gantt-timespan-today']
			}
		];

		var defaultPeriod = outboundService.getDefaultPeriod();
		var weekends = miscService.getAllWeekendsInPeriod(defaultPeriod);
	
		angular.forEach(weekends, function(we) {
			$scope.timespans.push({
				from: (we[0].isSame(defaultPeriod[0])) ? we[0].clone().subtract(1, 'day').format('YYYY-MM-DD') : we[0].clone().format('YYYY-MM-DD'),
				to: we[1].clone().add(1, 'day').format('YYYY-MM-DD')
			});
		});


		function setGanttOptions() {
			var defaultPeriod = outboundService.getDefaultPeriod();
			$scope.settings.ganttStartDate = defaultPeriod[0];
			return {
				headers: ['month', 'week'],
				fromDate: defaultPeriod[0],
				toDate: defaultPeriod[1]				
			};
		}

		function getGanttVisualization() {
			var defaultPeriod = outboundService.getDefaultPeriod();
			var ganttPeriod = {
				StartDate: { Date: defaultPeriod[0] },
				EndDate: { Date: defaultPeriod[1] }
			};

			var deferred = $q.defer();
			
			outboundService.getGanttVisualization(ganttPeriod, function success(data) {
				var ganttArr = [];
				if (data) data.forEach(function (ele, ind) {
					

					ganttArr[ind] = {};
					ganttArr[ind].name = ele.Name;
					ganttArr[ind].id = ele.Id;
					ganttArr[ind].tasks = [];
					ganttArr[ind].tasks[0] = {};
					ganttArr[ind].tasks[0].from =  miscService.getDateFromServer(ele.StartDate.Date);
					ganttArr[ind].tasks[0].to = miscService.getDateFromServer(moment(ele.EndDate.Date).add(1, 'days').toDate());
					ganttArr[ind].tasks[0].id = ele.Id;
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

		function updateGanttRowFromCampaignSummary(row, campaignSummary) {
			row.campaignNameClass = null;
			row.tasks[0].color = campaignSummary.IsScheduled ? '#C2E085' : '#66C2FF';
			campaignSummary.WarningInfo.forEach(function (warning) {
				if (warning.TypeOfRule == 'CampaignUnderSLA') {
					row.campaignNameClass = 'campaign-late';
				}
				if (warning.TypeOfRule == 'CampaignOverstaff') {
					row.campaignNameClass = 'campaign-early';
				}
			});
		}

		function updateWarningInfo(row, campaignSummary) {
			row.campaign.WarningInfo = campaignSummary.WarningInfo;
		}

		function updateAllCampaignGanttDisplay(campaignSummaryList) {
			campaignSummaryList.forEach(function(campaignSummary) {
				updateSingleCampaignGanttDisplay(campaignSummary);
			});
		}

		function updateSingleCampaignGanttDisplay(campaignSummary) {
			$scope.ganttData.forEach(function (dataRow, indx) {
				if (campaignSummary.Id == dataRow.id) {
					return updateGanttRowFromCampaignSummary(dataRow, campaignSummary);
				}
				if (campaignSummary.Id + '_expanded' == dataRow.id) {
					return updateWarningInfo(dataRow, campaignSummary);
				}
			});
		}
		return {
			init: init,
			readIndex: readIndex,
			getCommandCallback: getCommandCallback,
			getGraphData: getGraphData,
			setGanttOptions: setGanttOptions,
			getGanttVisualization: getGanttVisualization,
			updateGanttRowFromCampaignSummary: updateGanttRowFromCampaignSummary,
			updateAllCampaignGanttDisplay: updateAllCampaignGanttDisplay,
			updateSingleCampaignGanttDisplay: updateSingleCampaignGanttDisplay
		}
	}

})();