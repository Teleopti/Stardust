(function() {

	'use strict';

	angular.module('wfm.outbound')
		.controller('CampaignListGanttCtrl', ['$scope', 'OutboundToggles', 'outboundService', campaignListGanttCtrl]);

	function campaignListGanttCtrl($scope, OutboundToggles, outboundService) {

		$scope.initialized = false;
		$scope.$watch(function() {
			return OutboundToggles.ready;
		}, function(value) {
			if (value) {
				if (OutboundToggles.isGanttEnabled()) init();
			}
		});

		function init() {
			$scope.initialized = true;
			getGanttVisualization();
			$scope.ganttOptions = setGanttOptions();
		}

		

		function setGanttOptions(startDate,endDate) {
			var twoWeeksEarly = moment().subtract(2, "weeks").format();
			var fourWeeksAfter = moment().add(6, "weeks").format();
			return {
				headers: ['month', 'day'],
				fromDate: startDate ? startDate : twoWeeksEarly,
				toDate: endDate ? endDate : fourWeeksAfter
			}
		}

		function getGanttVisualization(startDate,endDate) {
			var twoWeeksEarly = moment().subtract(2, "weeks").format();
			var fourWeeksAfter = moment().add(6, "weeks").format();

			var ganttPeriod = {
				StartDate: { Date: startDate ? startDate : twoWeeksEarly },
				EndDate: { Date: endDate ? endDate : fourWeeksAfter }
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
				});
				$scope.ganttData = ganttArr;
			});
		}


	}

})();