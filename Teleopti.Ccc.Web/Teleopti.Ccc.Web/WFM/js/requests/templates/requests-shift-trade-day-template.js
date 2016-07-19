﻿(function () {
	'use strict';

	angular.module('wfm.requests').run([
        '$templateCache', function ($templateCache) {

			//  setup weekend display - to reduce DOM size, just render in the first column and make 100% absolute positioning
        	var template = '<div ng-repeat=\"day in grid.appScope.shiftTradeDayViewModels\" ' +
								'ng-if="day.isWeekend && rowRenderIndex == 0" ng-style=\"{\'left\': day.leftOffset}\" class=\'isWeekend\'>' +
					        '</div>';

			// setup shift trade day display
        	template += '<div style=\'position: relative\' ng-repeat=\"day in grid.appScope.shiftTradeScheduleViewModels[row.entity.Id]\">' +
						'		<div ng-style="{\'display\': \'inline-block\', \'position\': \'absolute\', \'left\': day.LeftOffset}\" >' +
						'			<div ng-click=\"grid.appScope.showShiftDetail($event,row.entity[\'PersonId\'],row.entity[\'PersonIdTo\'], day.date)\">' +
						'				<div class=\"shift-trade-cell-text\" ng-class=\"day.FromScheduleDayDetail.IsDayOff ? \'is-day-off\':\'\'\" ng-style=\"{\'background-color\': day.FromScheduleDayDetail.Color}\">' +
						'					{{::day.FromScheduleDayDetail.ShortName}}' +
						'					<md-tooltip>' +
						'						{{::day.FromScheduleDayDetail.Name}}' +
						'					</md-tooltip>' +
						'				</div> ' +
						'				<div class=\"shift-trade-cell-text\" ng-class=\"day.ToScheduleDayDetail.IsDayOff ? \'is-day-off\':\'\'\" ng-style=\"{\'background-color\': day.ToScheduleDayDetail.Color}\" >' +
						'					{{::day.ToScheduleDayDetail.ShortName}}' +
						'					<md-tooltip>' +
						'						{{::day.ToScheduleDayDetail.Name}}' +
						'					</md-tooltip>' +
						'				</div>' +
						'			</div> ' +
						'		</div>' +
					    '</div>';



        	$templateCache.put("shift-trade-day-template.html", template);
        }
	]);
})();

