"use strict";

!function () {
	angular.module('ui.grid').config(['$provide', function ($provide) {
		$provide.decorator('uiGridColumnMenuDirective', ['$timeout', 'gridUtil', 'uiGridConstants', 'uiGridColumnMenuService', '$document', '$delegate',
			function ($timeout, gridUtil, uiGridConstants, uiGridColumnMenuService, $document, $delegate) {
				var directive = $delegate[0];
				var originLink = directive.link;
				var newLink = function ($scope, $elm, $attrs, uiGridCtrl) {
					var self = this;
					uiGridColumnMenuService.initialize($scope, uiGridCtrl);
					$scope.defaultMenuItems = uiGridColumnMenuService.getDefaultMenuItems($scope);
					$scope.menuItems = $scope.defaultMenuItems;
					uiGridColumnMenuService.setColMenuItemWatch($scope);
					$scope.showMenu = function (column, $columnElement, event) {
						$scope.col = column;
						var colElementPosition = uiGridColumnMenuService.getColumnElementPosition($scope, column, $columnElement);
						if ($scope.menuShown) {
							$scope.colElement = $columnElement;
							$scope.colElementPosition = colElementPosition;
							$scope.hideThenShow = true;
							$scope.$broadcast('hide-menu', {
								originalEvent: event
							});
						}
						else {
							if (uiGridCtrl.grid.gridMenuScope.shown) {
								uiGridCtrl.grid.gridMenuScope.toggleMenu();
							}
							//self.shown = $scope.menuShown = true;
							uiGridColumnMenuService.repositionMenu($scope, column, colElementPosition, $elm, $columnElement);
							$scope.colElement = $columnElement;
							$scope.colElementPosition = colElementPosition;
							$scope.$broadcast('show-menu', {
								originalEvent: event
							});
						}
					};
					$scope.hideMenu = function (broadcastTrigger) {
						$scope.menuShown = false;
						if (!broadcastTrigger) {
							$scope.$broadcast('hide-menu');
						}
					};
					$scope.$on('menu-hidden', function () {
						if ($scope.hideThenShow) {
							delete $scope.hideThenShow;
							uiGridColumnMenuService.repositionMenu($scope, $scope.col, $scope.colElementPosition, $elm, $scope.colElement);
							$scope.$broadcast('show-menu');
							$scope.menuShown = true;
						}
						else {
							$scope.hideMenu(true);
							if ($scope.col) {
								gridUtil.focus.bySelector($document, '.ui-grid-header-cell.' + $scope.col.getColClass() + ' .ui-grid-column-menu-button', $scope.col.grid, false);
							}
						}
					});
					$scope.$on('menu-shown', function () {
						$timeout(function () {
							uiGridColumnMenuService.repositionMenu($scope, $scope.col, $scope.colElementPosition, $elm, $scope.colElement);
							delete $scope.colElementPosition;
							delete $scope.columnElement;
						}, 200);
					});
					$scope.sortColumn = function (event, dir) {
						event.stopPropagation();
						$scope.grid.sortColumn($scope.col, dir, true).then(function () {
							$scope.grid.refresh();
							$scope.hideMenu();
						});
					};
					$scope.unsortColumn = function () {
						$scope.col.unsort();
						$scope.grid.refresh();
						$scope.hideMenu();
					};
					var setFocusOnHideColumn = function () {
						$timeout(function () {
							var focusToGridMenu = function () {
								return gridUtil.focus.byId('grid-menu', $scope.grid);
							};
							var thisIndex;
							$scope.grid.columns.some(function (element, index) {
								if (angular.equals(element, $scope.col)) {
									thisIndex = index;
									return true;
								}
							});
							var previousVisibleCol;
							$scope.grid.columns.some(function (element, index) {
								if (!element.visible) {
									return false;
								}
								else if (index < thisIndex) {
									previousVisibleCol = element;
								}
								else if (index > thisIndex && !previousVisibleCol) {
									previousVisibleCol = element;
									return true;
								}
								else if (index > thisIndex && previousVisibleCol) {
									return true;
								}
							});
							if (previousVisibleCol) {
								var colClass = previousVisibleCol.getColClass();
								gridUtil.focus.bySelector($document, '.ui-grid-header-cell.' + colClass + ' .ui-grid-header-cell-primary-focus', true).then(angular.noop, function (reason) {
									if (reason !== 'canceled') {
										return focusToGridMenu();
									}
								});
							}
							else {
								focusToGridMenu();
							}
						});
					};
					$scope.hideColumn = function () {
						$scope.col.colDef.visible = false;
						$scope.col.visible = false;
						$scope.grid.queueGridRefresh();
						$scope.hideMenu();
						$scope.grid.api.core.notifyDataChange(uiGridConstants.dataChange.COLUMN);
						$scope.grid.api.core.raise.columnVisibilityChanged($scope.col);
						setFocusOnHideColumn();
					};
				};
				directive.compile = function () {
					return newLink;
				}
				return $delegate;
			}]);
	}]);
}();