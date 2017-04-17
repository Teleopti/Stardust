(function () {
    "use strict";

	// Bug #43683 seems caused by design problm of ui-grid.
	// DOM updating of grid menu items was block by something in IE.
	// I did not find root cause of this problem, but this modification works :-p
	// -- by xinfli
	angular.module("ui.grid").decorator("uiGridGridMenuService", ["$delegate", "$timeout", "uiGridConstants",
		function ($delegate, $timeout, uiGridConstants) {
			$delegate.toggleColumnVisibility = function(gridCol) {
				gridCol.colDef.visible = !(gridCol.colDef.visible === true || gridCol.colDef.visible === undefined);

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
