﻿(function() {
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

(function(){
		angular.module('wfm.requests').service('uiGridFixService', function(){
			var svc = this;

			svc.fixColumneMenuToggling = function(){
				//Add another 'click' event hanlder for '.ui-grid-column-menu-button' 
				//and manually trigger 'click' which is binded to document to close popup ColumnVisibility menu list.
				angular.element(document.querySelectorAll('.ui-grid-column-menu-button')).on('click', function(event){ 
					angular.element(document).triggerHandler('click');
				});
			}
		})
})();