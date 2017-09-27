(function() {
	"use strict";

	// Bug #43683 seems caused by design problm of ui-grid.
	// DOM updating of grid menu items was block by something in IE.
	// I did not find root cause of this problem, but this modification works :-p
	// -- by xinfli
	angular.module("ui.grid").decorator("uiGridGridMenuService", ["$delegate", "$timeout", "uiGridConstants",
		function($delegate, $timeout, uiGridConstants) {
			$delegate.toggleColumnVisibility = function(gridCol) {
				gridCol.colDef.visible = !(gridCol.colDef.visible === true || angular.isUndefined(gridCol.colDef.visible));

				$timeout(function() {
					gridCol.grid.refresh();
					gridCol.grid.api.core.notifyDataChange(uiGridConstants.dataChange.COLUMN);
					gridCol.grid.api.core.raise.columnVisibilityChanged(gridCol);
				}, 200);
			};
			return $delegate;
		}
	]);
})();



(function() {
	'use strict';

	//Overwriting getColumnElementPosition of uiGridColumnMenuService for fixing bug #45655 
	angular.module('ui.grid').decorator('uiGridColumnMenuService', ['$delegate', 'i18nService', 'uiGridConstants', 'gridUtil', function($delegate, i18nService, uiGridConstants, gridUtil) {
		$delegate.getColumnElementPosition = function($scope, column, $columnElement) {
			var positionData = {};
			positionData.left = $columnElement[0].offsetLeft;
			positionData.top = $columnElement[0].offsetTop;
			positionData.parentLeft = $columnElement[0].offsetParent.offsetLeft;

			// Get the grid scrollLeft
			positionData.offset = 0;
			if (column.grid.options.offsetLeft) {
				positionData.offset = column.grid.options.offsetLeft;
			}

			//positionData.height = gridUtil.elementHeight($columnElement, true);
			//positionData.width = gridUtil.elementWidth($columnElement, true);
			positionData.width = $columnElement[0].clientWidth;
			positionData.height = $columnElement[0].clientHeight;
			if (/MSIE/.test(navigator.userAgent) || /rv:11.0/i.test(navigator.userAgent)) {
				positionData.width = $columnElement[0].clientWidth - 20;
			}

			return positionData;
		};

		return $delegate;
  }]);
})();