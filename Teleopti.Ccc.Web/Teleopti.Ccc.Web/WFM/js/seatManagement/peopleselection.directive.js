'use strict';

(function () {
	function peopleSelectionList(uiGridConstants) {
		return {
			controller: "PeopleStartCtrl",
			scope: {
				selectedPeopleList: '=selectedPeople',
				reset : '='
			},
			templateUrl: 'js/people/html/people-list.html',
			link: function (scope, element, attrs) {
				scope.isSelectionMode = true;
				scope.gridOptions.enableGridMenu = false;
				scope.gridOptions.enableColumnMenus = false;
				scope.dynamicColumnLoaded = true;
				scope.gridOptions.exporterMenuCsv = false;

				scope.reset = scope.resetSearch;

				scope.gridOptions.columnDefs = [
					{ displayName: 'FirstName', field: 'FirstName', headerCellFilter: 'translate', cellClass: 'first-name', minWidth: 100 },
					{ displayName: 'LastName', field: 'LastName', headerCellFilter: 'translate', sort: { direction: uiGridConstants.ASC, priority: 0 }, minWidth: 100 },
					{ displayName: 'Team', field: 'Team', headerCellFilter: 'translate', enableSorting: false, minWidth: 100 }
				];
			}
		}
	}

	angular.module('wfm.people').directive("peopleSelectionList", ['uiGridConstants', peopleSelectionList]);
})();
