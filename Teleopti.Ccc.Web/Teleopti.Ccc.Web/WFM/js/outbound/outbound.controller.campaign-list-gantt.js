(function() {

	'use strict';

	angular.module('wfm.outbound')
		.controller('CampaignListGanttCtrl', [
			'$scope',
			'$filter',
			'OutboundToggles',
			'outboundService',
			'miscService',
			'outboundTranslationService',
			'outboundChartService',
			campaignListGanttCtrl]);

	function campaignListGanttCtrl($scope, $filter, OutboundToggles, outboundService, miscService, outboundTranslationService, outboundChartService) {

		$scope.isGanttEnabled = false;
		$scope.isLoadFinished = false;

		$scope.$watch(function() {
			return OutboundToggles.ready;
		}, function(value) {
			if (value) {
				if (OutboundToggles.isGanttEnabled()) {
					$scope.isGanttEnabled = true;
					init();
				}
			}
		});

		function init() {
			getGanttVisualization();
			$scope.ganttOptions = setGanttOptions();
		}
		
		$scope.headerFormats = {
			month: 'MMMM',
			week: function (column) {				
				return '<div class="week-days-header">'
				+ '<div class="week-start-day">' + column.date.format('D') + '</div>'
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
			if (c.expanded) {
				c.expanded = false;
				$scope.ganttData.splice(readIndex(c) + 1, 1);

			} else {
				c.expanded = true;
				var campaign = { Id: c.id, isLoadingData: true };
				var newDataRow = {
					id: c.id + '_expanded',
					height: '600px',
					expansion: true,
					campaign: campaign,
					collapse: function collapseExpansion(ev, index) {
						c.expanded = false;
						$scope.ganttData.splice(index, 1);
					}
				};
				var index = readIndex(c);
				if (index >= 0)
					$scope.ganttData.splice(index + 1, 0, newDataRow);
				getGraphData(campaign, $scope);
			}
		};

		function getGraphData(campaign, scope) {		
			outboundService.getCampaignSummary(campaign.Id, function (_campaign) {
				angular.extend(campaign, _campaign);
				outboundChartService.getCampaignVisualization(campaign.Id, function (data, translations, manualPlan, closedDays, backlog) {
					campaign.graphData = data;
					campaign.rawManualPlan = manualPlan;
					campaign.isManualBacklog = backlog;
					campaign.translations = translations;
					campaign.closedDays = closedDays;
					campaign.isLoadingData = false;
					scope.$broadcast('campaign.chart.refresh', campaign);
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


		function setGanttOptions(startDate, endDate) {
			var defaultPeriod = outboundService.getDefaultPeriod();
			return {
				headers: ['month', 'week'],
				fromDate: startDate ? startDate : defaultPeriod[0],
				toDate: endDate ? endDate : defaultPeriod[1]				
			};
		}

		function getGanttVisualization(startDate,endDate) {
			var defaultPeriod = outboundService.getDefaultPeriod();
			var ganttPeriod = {
				StartDate: { Date: startDate ? startDate : defaultPeriod[0] },
				EndDate: { Date: endDate ? endDate : defaultPeriod[1] }
			};
			
			outboundService.getGanttVisualization(ganttPeriod, function success(data) {
				var ganttArr = [];
				if (data) data.forEach(function (ele, ind) {
					var fromDate = miscService.getDateFromServer(ele.StartDate.Date);
					var toDate = miscService.getDateFromServer(ele.EndDate.Date);
					toDate.setDate(toDate.getDate() + 1);
					ganttArr[ind] = {};
					ganttArr[ind].name = ele.Name;
					ganttArr[ind].id = ele.Id;
					ganttArr[ind].tasks = [];
					ganttArr[ind].tasks[0] = {};
					ganttArr[ind].tasks[0].from = fromDate;
					ganttArr[ind].tasks[0].to = toDate;
					ganttArr[ind].tasks[0].id = ele.Id;
					ganttArr[ind].tasks[0].color = '#09F';
					ganttArr[ind].tasks[0].tooltips = {
					'enabled': true
					}
					ganttArr[ind].tasks[0].campaignName = ele.Name;					
				});
				$scope.ganttData = ganttArr;
				$scope.isLoadFinished = true;
			});
		}
	}

})();