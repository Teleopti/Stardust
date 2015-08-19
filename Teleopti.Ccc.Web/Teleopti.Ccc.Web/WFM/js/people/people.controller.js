'use strict';

angular.module('wfm.people')
	.constant('chunkSize', 50)
	.controller('PeopleCtrl', [
		'$scope', '$filter', '$state', '$document', '$translate', 'Upload', 'i18nService', 'uiGridConstants',
		'uiGridExporterConstants', '$timeout', '$q', 'Toggle', 'People', PeopleController
	]);

function PeopleController($scope, $filter, $state, $document, $translate, Upload, i18nService, uiGridConstants,
	uiGridExporterConstants, $timeout, $q, toggleSvc, peopleSvc) {
	$scope.searchResult = [];
	$scope.pageSize = 20;
	$scope.keyword = '';
	$scope.totalPages = 0;
	$scope.currentPageIndex = 1;
	$scope.searchKeywordChanged = false;
	$scope.advancedSearchForm = {};
	$scope.searchCriteriaDic = {};
	$scope.lang = i18nService.getCurrentLang();
	$scope.isImportUsersEnabled = false;
	$scope.isAdjustSkillEnabled = false;
	$scope.showImportPanel = false;

	$scope.dataInitialized = false;

	$scope.buttons = [{
		label: 'ImportUsers',
		icon: 'mdi-file',
		action: $scope.toggleImportPeople
	}];

	$scope.toggleImportPeople = function() {
		$scope.showImportPanel = !$scope.showImportPanel;
	};

	$scope.floatingButtonClick = function () {
		$scope.toggleImportPeople();
	}

	$scope.rowSelectionEnabled = function () {
		return $scope.isAdjustSkillEnabled;
	};

	var dynamicColumnLoaded = false;
	var paginationOptions = {
		pageNumber: 1,
		pageSize: 20,
		sortColumns: [{
			ColumnName: "LastName",
			SortASC: true
		}]
	};

	$scope.selectedPeopleList = [];
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
		columnDefs: [
			{ displayName: 'FirstName', field: 'FirstName', headerCellFilter: 'translate', cellClass: 'first-name',minWidth:100 },
			{
				displayName: 'LastName',
				field: 'LastName',
				headerCellFilter: 'translate',
				sort: {
					direction: uiGridConstants.ASC,
					priority: 0,
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
			return loadAllResults()
			.then(function () {
				getPage();
			});
		},
		paginationPageSize: 20,
		useExternalPagination: true,
		enablePaginationControls: false,
		onRegisterApi: function (gridApi) {
			$scope.gridApi = gridApi;
			gridApi.pagination.on.paginationChanged($scope, function (newPage, pageSize) {
				$scope.currentPageIndex = newPage;
				paginationOptions.pageNumber = newPage;
				paginationOptions.pageSize = pageSize;
				getPage();
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

	var selectPeople = function(rows) {
		angular.forEach(rows, function (row) {
			var selectedPerson = row.entity;
			if (row.isSelected && $scope.selectedPeopleList.indexOf(selectedPerson.PersonId) == -1) {
				$scope.selectedPeopleList.push(selectedPerson.PersonId);
			} else if (!row.isSelected && $scope.selectedPeopleList.indexOf(selectedPerson.PersonId) > -1) {
				for (var i = $scope.selectedPeopleList.length; i--;) {
					if ($scope.selectedPeopleList[i] === selectedPerson.PersonId) {
						$scope.selectedPeopleList.splice(i, 1);
					}
				}
			}
		});
		$scope.selectedCount = $scope.selectedPeopleList.length;
	}

	$scope.toggleRowSelectable = function () {
		$scope.gridOptions.enableFullRowSelection = $scope.rowSelectionEnabled();
		$scope.gridOptions.enableRowHeaderSelection = $scope.rowSelectionEnabled();
	};

	$scope.sortChanged = function (grid, sortColumns) {
		paginationOptions.sortColumns = [];

		for (var i = 0; i < sortColumns.length; i++) {
			var sortColumnName = sortColumns[i].name;
			for (var j = 0; j < $scope.gridOptions.columnDefs.length; j++) {
				if (sortColumnName === $scope.gridOptions.columnDefs[j].name) {
					paginationOptions.sortColumns.push({
						ColumnName: sortColumnName,
						SortASC: sortColumns[i].sort.direction == uiGridConstants.ASC
					});
					break;
				}
			}
		}

		getPage();
	};

	var getPage = function () {
		$scope.currentPageIndex = $scope.searchKeywordChanged ? 1 : paginationOptions.pageNumber;

		var sortColumnList = "";
		for (var i = 0; i < paginationOptions.sortColumns.length; i++) {
			var column = paginationOptions.sortColumns[i];
			sortColumnList = sortColumnList + column.ColumnName + ":" + column.SortASC + ";";
		};

		if (sortColumnList != "") {
			sortColumnList = sortColumnList.substring(0, sortColumnList.length - 1);
		}

		peopleSvc.search.query({
			keyword: $scope.keyword,
			pageSize: paginationOptions.pageSize,
			currentPageIndex: $scope.currentPageIndex,
			sortColumns: sortColumnList
		}).$promise.then(function (result) {
			$scope.searchResult = result.People;
			$scope.gridOptions.data = result.People;
			angular.forEach($scope.searchResult, function (person) {
				angular.forEach(person.OptionalColumnValues, function (val) {
					person[val.Key] = val.Value;
				});
			});

			if (!dynamicColumnLoaded) {
				angular.forEach(result.OptionalColumns, function (col) {
					var isFound = false;
					angular.forEach($scope.gridOptions.columnDefs, function (colDef) {
						if (colDef.field == col) {
							isFound = true;
							return;
						}
					});
					if (!isFound) {
						$scope.gridOptions.columnDefs.push({ displayName: col, field: col, headerCellFilter: 'translate', enableSorting: false, minWidth: 100 });
					}

				});
				
				dynamicColumnLoaded = true;
			}

			$scope.optionalColumns = result.OptionalColumns;
			$scope.totalPages = result.TotalPages;
			$scope.keyword = $scope.defautKeyword();
			$scope.searchKeywordChanged = false;
			$scope.gridOptions.totalItems = result.TotalPages * paginationOptions.pageSize;
		});
	};

	var allSearchTypes = [
		"FirstName",
		"LastName",
		"EmploymentNumber",
		"Organization",
		"Role",
		"Contract",
		"ContractSchedule",
		"ShiftBag",
		"PartTimePercentage",
		"Skill",
		"BudgetGroup",
		"Note"
	];

	function setSearchFormProperty(searchType, searchValue) {
		angular.forEach(allSearchTypes, function (propName) {
			if (propName.toUpperCase() == searchType.toUpperCase()) {
				$scope.advancedSearchForm[propName] = searchValue;
			}
		});
	}

	function parseSearchKeywordInputted() {
		$scope.advancedSearchForm = {};
		if ($scope.keyword.indexOf(':') != -1) {
			var searchTerms = $scope.keyword.split(',');
			angular.forEach(searchTerms, function (searchTerm) {
				var termSplitter = searchTerm.indexOf(':');
				if (termSplitter < 0) {
					return;
				}

				var searchType = searchTerm.substring(0, termSplitter).trim();
				var searchValue = searchTerm.substring(termSplitter + 1, searchTerm.length).trim();

				setSearchFormProperty(searchType, searchValue);
			});
		}
	}

	$scope.validateSearchKeywordChanged = function () {
		$scope.searchKeywordChanged = true;
		parseSearchKeywordInputted();
	};

	$scope.searchKeyword = function () {
		getPage();
	};

	var loadAllResults = function () {
		var sortColumnList = "";
		for (var i = 0; i < paginationOptions.sortColumns.length; i++) {
			var column = paginationOptions.sortColumns[i];
			sortColumnList = sortColumnList + column.ColumnName + ":" + column.SortASC + ";";
		};

		if (sortColumnList != "") {
			sortColumnList = sortColumnList.substring(0, sortColumnList.length - 1);
		}

		return peopleSvc.search.query({
			keyword: $scope.keyword,
			pageSize: $scope.gridOptions.totalItems,
			currentPageIndex: 1,
			sortColumns: sortColumnList
		}).$promise.then(function (result) {
			$scope.gridOptions.data = result.People;

			angular.forEach($scope.gridOptions.data, function (person) {
				angular.forEach(person.OptionalColumnValues, function (val) {
					person[val.Key] = val.Value;
				});
			});

			angular.forEach(result.OptionalColumns, function (col) {
				var isFound = false;
				angular.forEach($scope.gridOptions.columnDefs, function (colDef) {
					if (colDef.field == col) {
						isFound = true;
						return;
					}
				});
				if (!isFound) {
					$scope.gridOptions.columnDefs.push({ displayName: col, field: col, headerCellFilter: 'translate', enableSorting: false, minWidth: 100 });
				}
			});
		});
	}

	$scope.defautKeyword = function () {
		if ($scope.keyword == '' && $scope.gridOptions.data != undefined && $scope.gridOptions.data.length > 0) {
			return "\"" + $scope.gridOptions.data[0].Team.replace("/", "\" \"") + "\"";
		}
		return $scope.keyword;
	};

	$scope.getVisiblePageNumbers = function (start, end) {
		var displayPageCount = 5;
		var ret = [];
		if (!end) {
			end = start;
			start = 1;
		}

		var leftBoundary = start;
		var rightBoundary = end;
		if (end - start >= displayPageCount) {
			var currentPageIndex = $scope.currentPageIndex;

			if (currentPageIndex < displayPageCount - 1) {
				leftBoundary = 1;
				rightBoundary = displayPageCount;
			} else if (end - currentPageIndex < 3) {
				leftBoundary = end - displayPageCount + 1;
				rightBoundary = end;
			} else {
				leftBoundary = currentPageIndex - Math.floor(displayPageCount / 2) > 1 ? currentPageIndex - Math.floor(displayPageCount / 2) : 1;
				rightBoundary = currentPageIndex + Math.floor(displayPageCount / 2) > end ? end : currentPageIndex + Math.floor(displayPageCount / 2);
			}
		}

		for (var i = leftBoundary; i <= rightBoundary ; i++) {
			ret.push(i);
		}

		return ret;
	};

	$scope.prevPage = function () {
		if ($scope.currentPageIndex > 1) {
			$scope.currentPageIndex--;
			$scope.searchKeyword();
		}
	};

	$scope.nextPage = function () {
		if ($scope.currentPageIndex < $scope.totalPages) {
			$scope.currentPageIndex++;
			$scope.searchKeyword();
		}
	};

	$scope.setPage = function () {
		$scope.currentPageIndex = this.n;
		$scope.searchKeyword();
	};

	$scope.showAdvancedSearchOption = false;
	$scope.toggleAdvancedSearchOption = function ($event) {
		$scope.showAdvancedSearchOption = !$scope.showAdvancedSearchOption;
		$event.stopPropagation();
	};

	$scope.turnOffAdvancedSearch = function () {
		$scope.showAdvancedSearchOption = false;
	};

	function getSearchCriteria(title, value) {
		return value != undefined && value != "" ? title + ": " + value + ", " : "";
	}

	$scope.advancedSearch = function () {
		$scope.showAdvancedSearchOption = false;

		var keyword = "";
		angular.forEach(allSearchTypes, function (searchType) {
			// Change first letter to lowercase
			var title = searchType.charAt(0).toLowerCase() + searchType.slice(1);
			keyword += getSearchCriteria(title, $scope.advancedSearchForm[searchType]);
		});

		if (keyword != "") {
			keyword = keyword.substring(0, keyword.length - 2);
		}
		if (keyword != "" && keyword != $scope.keyword) {
			$scope.searchKeywordChanged = true;
		}
		$scope.keyword = keyword;

		getPage();
	}

	var promisesForDataInitialization = [];
	var promiseForAdvancedSearchToggle = toggleSvc.isFeatureEnabled.query({ toggle: 'WfmPeople_AdvancedSearch_32973' }).$promise;
	promiseForAdvancedSearchToggle.then(function (result) {
		$scope.isAdvancedSearchEnabled = result.IsEnabled;
	});
	promisesForDataInitialization.push(promiseForAdvancedSearchToggle);

	var promiseForImportUsersToggle = toggleSvc.isFeatureEnabled.query({ toggle: 'WfmPeople_ImportUsers_33665' }).$promise;
	promiseForImportUsersToggle.then(function (result) {
		$scope.isImportUsersEnabled = result.IsEnabled;
	});
	promisesForDataInitialization.push(promiseForImportUsersToggle);

	var promiseForAdjustSkillToggle = toggleSvc.isFeatureEnabled.query({ toggle: 'WfmPeople_AdjustSkill_34138' }).$promise;
	promiseForAdjustSkillToggle.then(function (result) {
		$scope.isAdjustSkillEnabled = result.IsEnabled;
	});
	promisesForDataInitialization.push(promiseForAdjustSkillToggle);

	$q.all(promisesForDataInitialization).then(function () {
		$scope.toggleRowSelectable();
		$scope.dataInitialized = true;
		$scope.searchKeyword();
	});
};
