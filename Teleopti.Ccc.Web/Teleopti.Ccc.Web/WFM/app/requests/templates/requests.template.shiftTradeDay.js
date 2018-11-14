(function() {
	'use strict';

	angular.module('wfm.requests').run(['$templateCache', requestsShiftTradeDayTemplate]);

	function requestsShiftTradeDayTemplate($templateCache) {
		//  setup weekend display - to reduce DOM size, just render in the first column and make 100% absolute positioning
		var template =
			'<div ng-repeat="day in grid.appScope.shiftTradeDayViewModels" ' +
			'ng-if="day.isWeekend && rowRenderIndex == 0" ng-style="{\'left\': day.leftOffset}" class=\'is-weekend\'>' +
			'</div>';

		// setup shift trade day display
		template +=
			'<div style=\'position: relative\' ng-repeat="day in grid.appScope.shiftTradeScheduleViewModels[row.entity.Id]">' +
			"		<div ng-style=\"{'display': 'inline-block', 'position': 'absolute', 'left': day.LeftOffset}\" >" +
			'			<div requests-shift-detail show-shift-detail="grid.appScope.showShiftDetail(params)" person-ids="[row.entity[\'PersonId\'],row.entity[\'PersonIdTo\']]" date="day.originalDate" target-timezone="day.targetTimezone">' +
			"				<div class=\"shift-trade-cell-text\" ng-class=\"day.FromScheduleDayDetail.IsDayOff ? 'is-day-off':''\" ng-style=\"{'background-color': day.FromScheduleDayDetail.Color, 'cursor': 'pointer'}\">" +
			'					{{::day.FromScheduleDayDetail.ShortName}}' +
			'					<md-tooltip>' +
			'						{{::day.FromScheduleDayDetail.Name}}' +
			'					</md-tooltip>' +
			'				</div> ' +
			"				<div class=\"shift-trade-cell-text\" ng-class=\"day.ToScheduleDayDetail.IsDayOff ? 'is-day-off':''\" ng-style=\"{'background-color': day.ToScheduleDayDetail.Color, 'cursor': 'pointer'}\" >" +
			'					{{::day.ToScheduleDayDetail.ShortName}}' +
			'					<md-tooltip>' +
			'						{{::day.ToScheduleDayDetail.Name}}' +
			'					</md-tooltip>' +
			'				</div>' +
			'			</div> ' +
			'		</div>' +
			'</div>';

		$templateCache.put('shift-trade-day-template.html', template);
	}
})();
