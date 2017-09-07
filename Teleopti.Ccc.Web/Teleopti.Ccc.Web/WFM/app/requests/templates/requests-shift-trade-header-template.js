(function () {
	'use strict';

	angular.module('wfm.requests').run(['$templateCache', requestsShiftTradeHeaderTemplate]);

		function requestsShiftTradeHeaderTemplate($templateCache) {
			var template =
				"<div role=\"rowgroup\" class=\"ui-grid-header\"><!-- theader -->" +
					"<div class=\"ui-grid-top-panel\">" +
					"	<div class=\"ui-grid-header-viewport\">" +
					"		<div class=\"ui-grid-header-canvas\">" +
					"			<div class=\"ui-grid-header-cell-wrapper\" ng-style=\"colContainer.headerCellWrapperStyle()\">" +
					"				<div role=\"row\" class=\"ui-grid-header-cell-row\" ng-class=\"grid.appScope.filterEnabled ? \'request-header-full-height\' : \'\' \"  >" +
					"					<div class=\"ui-grid-header-cell ui-grid-clearfix\" " +
					"						ng-if=\"!col.colDef.isShiftTradeDayColumn\"" +
					"						ng-repeat=\"col in colContainer.renderedColumns track by col.uid\" " +
					"						ui-grid-header-cell col=\"col\" " +
					"						render-index=\"$index\">" +
					"					</div>" +
					"					<div class=\"ui-grid-header-cell ui-grid-clearfix shift-trade-header-container\" " +
					"						ng-if=\"col.colDef.isShiftTradeDayColumn\"" +
					"						ng-repeat=\"col in colContainer.renderedColumns track by col.uid\" " +
					"						> " +
					"						<div class='shift-trade-header' ng-repeat=\"day in grid.appScope.shiftTradeDayViewModels\" ng-style=\"{\'left\': day.leftOffset}\" >" +
					"								<div class='shift-trade-header-start-of-week' ng-show=\"day.isStartOfWeek && !day.isLatestDayOfPeriod\">{{day.shortDate}}</div>" +
					"								<div ng-show=\"!day.isStartOfWeek || day.isLatestDayOfPeriod\"> &nbsp;</div>" +
					"								<div class='shift-trade-header-day-number'>{{day.dayNumber}}</div>" +
					"						</div>" +
					"					</div>" +
					"				</div>" +
					"			</div>" +
					"		</div>" +
					"	</div>" +
					"</div>" +
					"</div>";

			$templateCache.put("shift-trade-header-template.html", template);
		}
})();
