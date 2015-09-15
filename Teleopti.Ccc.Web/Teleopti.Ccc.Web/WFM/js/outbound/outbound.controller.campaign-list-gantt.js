(function() {

	'use strict';

	angular.module('wfm.outbound')
		.controller('CampaignListGanttCtrl', ['$scope', 'OutboundToggles', 'outboundService', campaignListGanttCtrl]);

	function campaignListGanttCtrl($scope, OutboundToggles, outboundService) {

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
			month: 'MMMM YYYY',
			week: function (column) {				
				return '<div class="week-days-header">'
				+ '<div class="week-start-day">' + column.date.format('D') + '</div>'
				+ '<div></div>'
				+ '<div class="week-end-day">'  + '</div>'
				+ '</div>';
			}
		};

		$scope.tooltipContent = '<div><div><strong>{{task.model.campaignName}}</strong></div>' +
			'<small>' +
			'{{task.isMilestone() === true && getFromLabel() || getFromLabel() + \' - \' + getToLabel()}}' +
			'</small></div>';
		
		function setGanttOptions(startDate,endDate) {
			var periodStart = moment().subtract( 1, "months").date(1).format();
			var periodEnd = moment().add(2, "months").date(1).subtract(1, 'days').format();		
			return {
				headers: ['month', 'week'],
				fromDate: startDate ? startDate : periodStart,
				toDate: endDate ? endDate : periodEnd
			}
		}

		function getGanttVisualization(startDate,endDate) {
			var periodStart = moment().subtract(1, "months").date(1).format();
			var periodEnd = moment().add(2, "months").date(1).subtract(1, 'days').format();

			var ganttPeriod = {
				StartDate: { Date: startDate ? startDate : periodStart },
				EndDate: { Date: endDate ? endDate : periodEnd }
			};
			
			outboundService.getGanttVisualization(ganttPeriod, function success(data) {
				var ganttArr = [];
				if (data) data.forEach(function (ele, ind) {
					ganttArr[ind] = {};
					ganttArr[ind].name = ele.Name;
					ganttArr[ind].id = ele.Id;
					ganttArr[ind].tasks = [];
					ganttArr[ind].tasks[0] = {};
					ganttArr[ind].tasks[0].from = ele.StartDate.Date;
					ganttArr[ind].tasks[0].to = ele.EndDate.Date;
					ganttArr[ind].tasks[0].id = ele.Id;
					ganttArr[ind].tasks[0].color = '#09F';
					ganttArr[ind].tasks[0].tooltips = {
					'enabled': true,
					'dateFormat': 'YYYY-MM-DD'					
					}
					ganttArr[ind].tasks[0].campaignName = ele.Name;
				});
				$scope.ganttData = ganttArr;
				$scope.isLoadFinished = true;
			});
		}


	}

})();