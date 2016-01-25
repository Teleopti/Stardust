'use strict';

angular.module('wfm.people')
	.constant('chunkSize', 50)
	.controller('PeopleStartCtrl', [
		'$scope', '$filter', '$state', '$stateParams', '$translate', 'i18nService', 'uiGridConstants',
		'uiGridExporterConstants', '$q', 'Toggle', 'People', PeopleStartController
	]);

function PeopleStartController($scope, $filter, $state, $stateParams, $translate, i18nService, uiGridConstants,
	uiGridExporterConstants, $q, toggleSvc, peopleSvc) {
	$scope.searchResult = [];
	$scope.pageSize = 20;
	$scope.searchOptions = {
		keyword: $stateParams.currentKeyword != undefined ? $stateParams.currentKeyword : "",
		searchKeywordChanged: false,
		isAdvancedSearchEnabled: false
	}
	$scope.lang = i18nService.getCurrentLang();
	$scope.isImportUsersEnabled = false;
	$scope.isAdjustSkillEnabled = false;
	$scope.showImportPanel = false;
	$scope.getCurrentPageSelectedRowsLength = function () {
		return $scope.gridOptions.data.length;
	}
	$scope.selectedCount = function () {
		return $scope.selectedPeopleList.length;
	};
	$scope.dataInitialized = false;
	$scope.selectAllVisible = false;
	$scope.commands = [
		{
			label: 'ImportUsers',
			icon: 'mdi-file',
			action: function () {
				$scope.toggleImportPeople();
			},
			active: function () {
				return $scope.isImportUsersEnabled;
			}
		}, {
			label: 'AdjustSkill',
			icon: 'mdi-package',
			action: function () {
				$scope.gotoCartView('AdjustSkill');
			},
			active: function () {
				return $scope.isAdjustSkillEnabled && ($scope.selectedCount() > 0);
			}
		}, {
			label: "ChangeShiftBag",
			icon: "mdi-package",
			action: function () {
				$scope.gotoCartView('ChangeShiftBag');
			},
			active: function () {
				return $scope.isAdjustSkillEnabled && ($scope.selectedCount() > 0);
			}
		}
	];

	$scope.toggleImportPeople = function () {
		$scope.showImportPanel = !$scope.showImportPanel;
	};

	$scope.rowSelectionEnabled = function () {
		return $scope.isAdjustSkillEnabled;
	};

	$scope.dynamicColumnLoaded = false;

	var stateParamsPagination = $stateParams.paginationOptions != undefined ? $stateParams.paginationOptions : {};

	$scope.paginationOptions = {
		pageNumber: stateParamsPagination.pageNumber != undefined ? stateParamsPagination.pageNumber : 1,
		pageSize: stateParamsPagination.pageSize != undefined ? stateParamsPagination.pageSize : 20,
		totalPages:0,
		sortColumns: stateParamsPagination.sortColumns != undefined ? stateParamsPagination.sortColumns : [
			{
				ColumnName: "LastName",
				SortASC: true
			}
		]
	};

	$scope.selectedPeopleList = $stateParams.selectedPeopleIds !== undefined ? $stateParams.selectedPeopleIds : [];

	$scope.gridOptions = {
		enableFullRowSelection: false,
		enableRowHeaderSelection: false,
		exporterMenuCsv: true,
		exporterCsvFilename: 'peoples.csv',
		exporterOlderExcelCompatibility: true,
		exporterHeaderFilter: $filter('translate'),
		exporterMenuPdf: false,
		enableGridMenu: true,
		useExternalSorting: true,
		enableColumnResizing: true,
		enableHorizontalScrollbar: function() {
			return columnDefs.length + $scope.optionalColumns > 8 ? true : false;
		},
		columnDefs: [
			{ displayName: 'FirstName', field: 'FirstName', headerCellFilter: 'translate', cellClass: 'first-name', minWidth: 100 },
			{
				displayName: 'LastName',
				field: 'LastName',
				headerCellFilter: 'translate',
				sort: {
					direction: uiGridConstants.ASC,
					priority: 0
				},
				minWidth: 100
			},
			{ displayName: 'EmployeeNo', field: 'EmploymentNumber', headerCellFilter: 'translate', minWidth: 100 },
			{ displayName: 'Team', field: 'Team', headerCellFilter: 'translate', enableSorting: false, minWidth: 100 },
			{ displayName: 'Email', field: 'Email', headerCellFilter: 'translate', enableSorting: false, minWidth: 100 },
			{ displayName: 'TerminalDate', field: 'LeavingDate', headerCellFilter: 'translate', enableSorting: false, minWidth: 100 }
		],
		gridMenuTitleFilter: $translate,
		exporterAllDataFn: function () {
			return loadAllResults(function (result) {
				$scope.gridOptions.data = result.People;

				angular.forEach($scope.gridOptions.data, function (person) {
					angular.forEach(person.OptionalColumnValues, function (val) {
						person[val.Key] = val.Value;
					});
				});

				angular.forEach(result.OptionalColumns, function (col) {
					var isFound = false;
					angular.forEach($scope.gridOptions.columnDefs, function (colDef) {
						if (colDef.field === col) {
							isFound = true;
						}
					});
					if (!isFound) {
						$scope.gridOptions.columnDefs.push({
							displayName: col,
							field: col,
							headerCellFilter: 'translate',
							enableSorting: false,
							minWidth: 100
						});
					}
				});
			})
				.then(function () {
					getPage(1);
				});
		},
		paginationPageSize: 20,
		useExternalPagination: true,
		enablePaginationControls: false,
		onRegisterApi: function (gridApi) {
			$scope.gridApi = gridApi;
			gridApi.pagination.on.paginationChanged($scope, function (newPage, pageSize) {
				$scope.paginationOptions.pageNumber = newPage;
				$scope.paginationOptions.pageSize = pageSize;
				getPage(newPage);
			});
			gridApi.selection.on.rowSelectionChanged($scope, function (row) {
				selectPeople([row]);
			});

			gridApi.selection.on.rowSelectionChangedBatch($scope, function (rows) {
				selectPeople(rows);
			});

			$scope.gridApi.core.on.sortChanged($scope, $scope.sortChanged);
		}
	};

	var getSelectedPeople = function () {
		return $scope.selectedPeopleList;
	};

	$scope.gotoCartView = function (commandTag) {
		$state.go("people.selection", {
			selectedPeopleIds: getSelectedPeople(),
			commandTag: commandTag,
			currentKeyword: $scope.searchOptions.keyword,
			paginationOptions: $scope.paginationOptions
		});
	};

	var selectPeople = function (rows) {
		angular.forEach(rows, function (row) {
			var selectedPerson = row.entity;
			var personIndex = $scope.selectedPeopleList.indexOf(selectedPerson.PersonId);
			if (row.isSelected && personIndex === -1) {
				$scope.selectedPeopleList.push(selectedPerson.PersonId);
			} else if (!row.isSelected && personIndex > -1) {
				$scope.selectedPeopleList.splice(personIndex, 1);
			}
		});

		var allRowsInCurrentPageSelected = $scope.gridApi.selection.getSelectedRows().length === $scope.getCurrentPageSelectedRowsLength();
		var allRowsSelected = $scope.selectedCount() === $scope.callBackResultLength;
		var rowInCurrentPageSelected = $scope.getCurrentPageSelectedRowsLength() > 0;

		if (allRowsInCurrentPageSelected) {
			$scope.gridApi.selection.selectAllRows();
		} else {
			$scope.gridApi.grid.selection.selectAll = false;
		}

		$scope.selectAllVisible = $scope.paginationOptions.totalPages > 1 && rowInCurrentPageSelected && !allRowsSelected;
	};

	$scope.selectAllMatch = function () {
		loadAllResults(function (result) {
			angular.forEach(result.People, function (person) {
				if ($scope.selectedPeopleList.indexOf(person.PersonId) === -1)
					$scope.selectedPeopleList.push(person.PersonId);
			});
			setPeopleSelectionStatus();
			$scope.selectAllVisible = false;

		});
	};

	function setPeopleSelectionStatus() {
		for (var i = 0; i < $scope.getCurrentPageSelectedRowsLength() ; i++) {
			var personId = $scope.gridOptions.data[i].PersonId;
			if ($scope.selectedPeopleList.indexOf(personId) > -1) {
				$scope.gridApi.grid.modifyRows($scope.gridOptions.data);
				$scope.gridApi.selection.selectRow($scope.gridOptions.data[i]);
				if (!$scope.gridApi.selection.getSelectedRows().length) {
					$scope.gridApi.grid.selection.selectAll = false;
				}
			}
		}
	};

	$scope.clearCart = function () {
		$scope.selectedPeopleList = [];
		$scope.gridApi.selection.clearSelectedRows();
	};

	$scope.toggleRowSelectable = function () {
		$scope.gridOptions.enableFullRowSelection = $scope.rowSelectionEnabled();
		$scope.gridOptions.enableRowHeaderSelection = $scope.rowSelectionEnabled();
	};

	$scope.sortChanged = function (grid, sortColumns) {
		$scope.paginationOptions.sortColumns = [];

		for (var i = 0; i < sortColumns.length; i++) {
			var sortColumnName = sortColumns[i].name;
			for (var j = 0; j < $scope.gridOptions.columnDefs.length; j++) {
				if (sortColumnName === $scope.gridOptions.columnDefs[j].name) {
					$scope.paginationOptions.sortColumns.push({
						ColumnName: sortColumnName,
						SortASC: sortColumns[i].sort.direction === uiGridConstants.ASC
					});
					break;
				}
			}
		}

		getPage(1);
	};
	$scope.getPageData = function (pageIndex) {
		$scope.gridApi.pagination.seek(pageIndex);
	} 
	function getPage(pageIndex) {
		$scope.paginationOptions.pageNumber = $scope.searchOptions.searchKeywordChanged ? 1 : pageIndex;

		var sortColumnList = "";
		for (var i = 0; i < $scope.paginationOptions.sortColumns.length; i++) {
			var column = $scope.paginationOptions.sortColumns[i];
			sortColumnList = sortColumnList + column.ColumnName + ":" + column.SortASC + ";";
		}

		if (sortColumnList !== "") {
			sortColumnList = sortColumnList.substring(0, sortColumnList.length - 1);
		}

		peopleSvc.search.query({
			keyword: $scope.searchOptions.keyword,
			pageSize: $scope.paginationOptions.pageSize,
			currentPageIndex: $scope.paginationOptions.pageNumber,
			sortColumns: sortColumnList
		}).$promise.then(function (result) {
			$scope.searchResult = result.People ? result.People : [];
			$scope.gridOptions.data = result.People ? result.People : [];
			$scope.gridOptions.paginationCurrentPage = $scope.paginationOptions.pageNumber;
			angular.forEach($scope.searchResult, function (person) {
				angular.forEach(person.OptionalColumnValues, function (val) {
					person[val.Key] = val.Value;
				});

			});

			if (!$scope.dynamicColumnLoaded) {
				angular.forEach(result.OptionalColumns, function (col) {
					var isFound = false;
					angular.forEach($scope.gridOptions.columnDefs, function (colDef) {
						if (colDef.field === col) {
							isFound = true;
						}
					});
					if (!isFound) {
						$scope.gridOptions.columnDefs.push({
							name: col,
							displayName: col,
							field: col,
							headerCellFilter: 'translate',
							enableSorting: false,
							minWidth: 100
						});
					}

				});

				$scope.dynamicColumnLoaded = true;
			}
			setPeopleSelectionStatus();
			$scope.optionalColumns = result.OptionalColumns;
			$scope.paginationOptions.totalPages = result.TotalPages;
			$scope.searchOptions.keyword = $scope.defautKeyword();
			$scope.searchOptions.searchKeywordChanged = false;
			$scope.gridOptions.totalItems = result.TotalPages * $scope.paginationOptions.pageSize;
		});
	}

	$scope.searchKeyword = function () {
		getPage($scope.paginationOptions.pageNumber);
		if ($scope.paginationOptions.totalPages > 1) {
			$scope.selectAllVisible = true;
		}
	};

	var loadAllResults = function (successCallback) {
		var sortColumnList = "";
		for (var i = 0; i < $scope.paginationOptions.sortColumns.length; i++) {
			var column = $scope.paginationOptions.sortColumns[i];
			sortColumnList = sortColumnList + column.ColumnName + ":" + column.SortASC + ";";
		}

		if (sortColumnList !== "") {
			sortColumnList = sortColumnList.substring(0, sortColumnList.length - 1);
		}

		return peopleSvc.search.query({
			keyword: $scope.searchOptions.keyword,
			pageSize: $scope.gridOptions.totalItems,
			currentPageIndex: 1,
			sortColumns: sortColumnList
		}).$promise.then(function (result) {
			successCallback(result);
			$scope.callBackResultLength = result.People.length;
		}
		);
	};

	$scope.defautKeyword = function () {
		if ($scope.searchOptions.keyword === '' && $scope.gridOptions.data !== undefined && $scope.getCurrentPageSelectedRowsLength() > 0) {
			return "\"" + $scope.gridOptions.data[0].Team.replace("/", "\" \"") + "\"";
		}
		return $scope.searchOptions.keyword;
	};

	$scope.searchOptions.isAdvancedSearchEnabled = toggleSvc.WfmPeople_AdvancedSearch_32973;
	$scope.isImportUsersEnabled = toggleSvc.WfmPeople_ImportUsers_33665;
	$scope.isAdjustSkillEnabled = toggleSvc.WfmPeople_AdjustSkill_34138;

	$scope.toggleRowSelectable();
	$scope.dataInitialized = true;
	$scope.searchKeyword();
}