(function () {
	'use strict';

	angular.module('wfm.requests').run([
        '$templateCache', function ($templateCache) {
			var template =
				'<div role=\"rowgroup\" class=\"ui-grid-header\">' +
					'	<div class=\"ui-grid-top-panel\">' +
					'		<div class=\"ui-grid-header-viewport\">' +
					'			<div class=\"ui-grid-header-canvas\">' +
					'				<div class=\"ui-grid-header-cell-wrapper\" ng-style=\"colContainer.headerCellWrapperStyle()\">' +
					'					<div role=\"row\" class=\"ui-grid-header-cell-row\" ng-class=\"grid.appScope.getShiftTradeHeaderClass()\">' +
					'						<div class=\"ui-grid-header-cell ui-grid-clearfix ui-grid-category\"' +
					'							 ng-repeat=\"cat in grid.options.category\"' +
					'							 ng-if=\"cat.visible && (colContainer.renderedColumns | filter:{ colDef:{category: cat.name} }).length>0\">' +
					'							<div ng-if=\"!cat.suppressCategoryHeader\" class=\"ui-grid-category-text\">{{cat.name}}</div>' +
					'							<div ng-if=\"cat.suppressCategoryHeader\" class=\"ui-grid-category-text\">&nbsp;</div>' +
					'							<div class=\"ui-grid-header-cell ui-grid-clearfix ui-grid-category-weekday\"' +
					'								 ng-repeat=\"col in colContainer.renderedColumns | filter:{ colDef:{category: cat.name} }\"' +
					'								 ui-grid-header-cell' +
					'								 col=\"col\"' +
					'								 render-index=\"$index\">' +
					'							</div>' +
					'						</div>' +
					'						<!-- !cat.visible && -->' +
					'						<div class=\"ui-grid-header-cell ui-grid-clearfix\"' +
					'							 ng-if=\"col.colDef.category === undefined\"' +
					'							 ng-repeat=\"col in colContainer.renderedColumns track by col.uid\"' +
					'							 ui-grid-header-cell' +
					'							 col=\"col\"' +
					'							 render-index=\"$index\">' +
					'						</div>' +
					'					</div>' +
					'				</div>' +
					'			</div>' +
					'		</div>' +
					'	</div>' +
					'</div>';

        	$templateCache.put("shift-trade-header-template.html", template);
        }
	]);
})();
