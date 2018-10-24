'use strict';

(function() {
	angular
		.module('wfm.peopleold')
		.directive('peopleSelectionList', ['$translate', 'uiGridConstants', peopleSelectionList]);

	function peopleSelectionList($translate, uiGridConstants) {
		return {
			controller: 'PeopleStartCtrl',
			scope: {
				selectedPeopleList: '=selectedPeople',
				clearSelectedPeople: '='
			},
			templateUrl: 'app/peopleold/html/people-list.html',
			link: function(scope, element, attrs) {
				scope.gridOptions.enableGridMenu = false;
				scope.gridOptions.enableColumnMenus = false;
				scope.dynamicColumnLoaded = true;
				scope.gridOptions.exporterMenuCsv = false;
				scope.gridOptions.enableFullRowSelection = true;
				scope.gridOptions.enableRowHeaderSelection = true;
				scope.clearSelectedPeople = scope.clearCart;

				scope.gridOptions.columnDefs = [
					{
						displayName: $translate.instant('FirstName'),
						field: 'FirstName',
						cellClass: 'first-name',
						minWidth: 100
					},
					{
						displayName: $translate.instant('LastName'),
						field: 'LastName',
						sort: { direction: uiGridConstants.ASC, priority: 0 },
						minWidth: 100
					},
					{
						displayName: $translate.instant('Team'),
						field: 'Team',
						enableSorting: false,
						minWidth: 100
					}
				];
			}
		};
	}
})();
