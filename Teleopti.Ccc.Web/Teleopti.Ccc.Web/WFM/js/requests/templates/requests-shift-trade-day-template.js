(function () {
	'use strict';

	angular.module('wfm.requests').run([
        '$templateCache', function ($templateCache) {
			var template =
				'<div ng-init=\"matchingShiftTradeDays = (row.entity[\'ShiftTradeDays\'] | filterShiftTradeDetailDisplay:col.colDef.field)\"' +
					'	 ng-class=\"col.colDef.isWeekend? \'isWeekend\':\'\'\">' +
					'	<table ng-if=\"matchingShiftTradeDays.length == 0\" style=\"width: 100%;\">' +
					'		<tr class=\"shift-trade-row\">' +
					'			<td class=\"shift-trade-cell\" ng-style=\"{\'background-color\': \'$white\'}\"></td>' +
					'		</tr>' +
					'		<tr class=\"shift-trade-row\">' +
					'			<td class=\"shift-trade-cell\" ng-style=\"{\'background-color\': \'$white\'}\"></td>' +
					'		</tr>' +
					'	</table>' +
					'	<table ng-click=\"grid.appScope.showShiftDetail($event,row.entity[\'PersonId\'],row.entity[\'PersonIdTo\'],matchingShiftTradeDays[0].Date)\" ng-if=\"matchingShiftTradeDays.length == 1\" style=\"width:100%\" ng-style=\"{cursor: grid.appScope.showRelevantInfo ? \'pointer\' : \'default\' }\">' +
					'		<tr class=\"shift-trade-row\" ng-repeat=\"shiftTradeDay in  matchingShiftTradeDays\">' +
					'			<td class=\"shift-trade-cell\">' +
					'				<div class=\"shift-trade-cell-text\"' +
					'					 ng-if=\"grid.appScope.shouldDisplayShiftTradeDayDetail(shiftTradeDay.FromScheduleDayDetail)\"' +
					'					 ng-class=\"grid.appScope.isDayOff(shiftTradeDay.FromScheduleDayDetail) ? \'is-day-off\':\'\'\"' +
					'					 ng-style=\"{\'background-color\': shiftTradeDay.FromScheduleDayDetail.Color}\">' +
					'					{{shiftTradeDay.FromScheduleDayDetail.ShortName}}' +
					'					<md-tooltip>' +
					'						{{shiftTradeDay.FromScheduleDayDetail.Name}}' +
					'					</md-tooltip>' +
					'				</div>' +
					'			</td>' +
					'		</tr>' +
					'		<tr class=\"shift-trade-row\" ng-repeat=\"shiftTradeDay in  matchingShiftTradeDays\">' +
					'			<td class=\"shift-trade-cell\">' +
					'				<div class=\"shift-trade-cell-text\"' +
					'					 ng-if=\"grid.appScope.shouldDisplayShiftTradeDayDetail(shiftTradeDay.ToScheduleDayDetail)\"' +
					'					 ng-class=\"grid.appScope.isDayOff(shiftTradeDay.ToScheduleDayDetail) ? \'is-day-off\':\'\'\"' +
					'					 ng-style=\"{\'background-color\': shiftTradeDay.ToScheduleDayDetail.Color}\">' +
					'					{{shiftTradeDay.ToScheduleDayDetail.ShortName}}' +
					'					<md-tooltip>' +
					'						{{shiftTradeDay.ToScheduleDayDetail.Name}}' +
					'					</md-tooltip>' +
					'				</div>' +
					'			</td>' +
					'		</tr>' +
					'	</table>' +
					'</div>';

        	$templateCache.put("shift-trade-day-template.html", template);
        }
	]);
})();
