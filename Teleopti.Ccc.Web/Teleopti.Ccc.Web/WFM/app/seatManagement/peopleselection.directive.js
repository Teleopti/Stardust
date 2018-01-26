﻿'use strict';

(function () {
	function peopleSelectionList(uiGridConstants) {
		return {
			controller: "PeopleStartCtrl",
			scope: {
				selectedPeopleList: '=selectedPeople',
				clearSelectedPeople : '='
			},
			templateUrl: 'app/people/html/people-list.html',
			link: function (scope, element, attrs) {
				scope.gridOptions.enableGridMenu = false;
				scope.gridOptions.enableColumnMenus = false;
				scope.dynamicColumnLoaded = true;
				scope.gridOptions.exporterMenuCsv = false;
				scope.gridOptions.enableFullRowSelection = true;
				scope.gridOptions.enableRowHeaderSelection = true;
				scope.clearSelectedPeople = scope.clearCart;

				scope.gridOptions.columnDefs = [
					{ displayName: 'FirstName', field: 'FirstName', headerCellFilter: 'translate', cellClass: 'first-name', minWidth: 100 },
					{ displayName: 'LastName', field: 'LastName', headerCellFilter: 'translate', sort: { direction: uiGridConstants.ASC, priority: 0 }, minWidth: 100 },
					{ displayName: 'Team', field: 'Team', headerCellFilter: 'translate', enableSorting: false, minWidth: 100 }
				];
			}
		}
	}

	angular.module('wfm.peopleold').directive("peopleSelectionList", ['uiGridConstants', peopleSelectionList]);
})();
