'use strict';

angular.module('wfm.people')
	.constant('chunkSize', 50)
	.controller('PeopleCtrl', [
		'$scope', '$filter', '$state', '$document', '$translate', 'Upload', 'i18nService', 'uiGridConstants', 'PeopleSearch', PeopleController
	]);

function PeopleController($scope, $filter, $state, $document, $translate, Upload, i18nService, uiGridConstants, SearchSvrc) {
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
	$scope.showImportPanel = false;

	$scope.buttons = [{
		label: 'Import Users',
		icon: 'mdi-file',
		action: toggleImportPeople
	}];

	function toggleImportPeople() {
		$scope.showImportPanel = !$scope.showImportPanel;
	};

	$scope.floatingButtonClick = function(action) {
		action();
	} 
	
	var dynamicColumnLoaded = false;
	var paginationOptions = {
		pageNumber: 1,
		pageSize: 20,
		sortColumns: [{
			ColumnName: "LastName",
			SortASC: true
		}]
	};
	$scope.gridOptions = {
		exporterMenuCsv: true,
		exporterCsvFilename: 'peoples.csv',
		exporterOlderExcelCompatibility: true,
		exporterHeaderFilter: $filter('translate'),
		exporterMenuPdf: false,
		enableGridMenu: true,
		useExternalSorting: true,
		enableColumnResizing: true,
		columnDefs: [
			{ displayName: 'FirstName', field: 'FirstName', headerCellFilter: 'translate', cellClass: 'first-name' },
			{
				displayName: 'LastName',
				field: 'LastName',
				headerCellFilter: 'translate',
				sort: {
					direction: uiGridConstants.ASC,
					priority: 0,
				}
			},
			{ displayName: 'EmployeeNo', field: 'EmploymentNumber', headerCellFilter: 'translate' },
			{ displayName: 'Team', field: 'Team', headerCellFilter: 'translate', enableSorting: false },
			{ displayName: 'Email', field: 'Email', headerCellFilter: 'translate', enableSorting: false  },
			{ displayName: 'TerminalDate', field: 'LeavingDate', headerCellFilter: 'translate', enableSorting: false }
		],
		gridMenuTitleFilter: $translate,
		exporterAllDataFn: function() {
			return loadAllResults()
			.then(function () {
				getPage();
			});
		},
		paginationPageSize: 20,
		useExternalPagination: true,
		enablePaginationControls: false,
		onRegisterApi: function(gridApi) {
			$scope.gridApi = gridApi;
			gridApi.pagination.on.paginationChanged($scope, function(newPage, pageSize) {
				$scope.currentPageIndex = newPage;
				paginationOptions.pageNumber = newPage;
				paginationOptions.pageSize = pageSize;
				getPage();
			});
			$scope.gridApi.core.on.sortChanged($scope, $scope.sortChanged);
		}
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
			var col = paginationOptions.sortColumns[i];
			sortColumnList = sortColumnList + col.ColumnName + ":" + col.SortASC + ";";
		};

		if (sortColumnList != "") {
			sortColumnList = sortColumnList.substring(0, sortColumnList.length - 1);
		}

		SearchSvrc.search.query({
			keyword: $scope.keyword,
			pageSize: paginationOptions.pageSize,
			currentPageIndex: $scope.currentPageIndex,
			sortColumns: sortColumnList
		}).$promise.then(function (result) {
			$scope.searchResult = result.People;
			$scope.gridOptions.data = result.People;
			angular.forEach($scope.searchResult, function (person) {
				angular.forEach(person.OptionalColumnValues, function(val) {
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
						$scope.gridOptions.columnDefs.push({ displayName: col, field: col, headerCellFilter: 'translate', enableSorting: false });
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

	getPage();

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
			var col = paginationOptions.sortColumns[i];
			sortColumnList = sortColumnList + col.ColumnName + ":" + col.SortASC + ";";
		};

		if (sortColumnList != "") {
			sortColumnList = sortColumnList.substring(0, sortColumnList.length - 1);
		}

		return SearchSvrc.search.query({
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
					$scope.gridOptions.columnDefs.push({ displayName: col, field: col, headerCellFilter: 'translate', enableSorting: false });
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
	
	SearchSvrc.isFeatureEnabled.query({ toggle: 'WfmPeople_AdvancedSearch_32973' }).$promise.then(function (result) {
		$scope.isAdvancedSearchEnabled = result.IsEnabled;
	});

	SearchSvrc.isFeatureEnabled.query({ toggle: 'WfmPeople_ImportUsers_33665' }).$promise.then(function (result) {
		$scope.isImportUsersEnabled = result.IsEnabled;
	});

	$scope.searchKeyword();

	$scope.files = [];
	$scope.$watch('files', function () {
			$scope.upload($scope.files);
		});
	$scope.log = '';

	$scope.upload = function (files) {
		if (files && files.length) {
			for (var i = 0; i < files.length; i++) {
				var file = files[i];
				Upload.upload({
					url: 'https://angular-file-upload-cors-srv.appspot.com/upload',
					fields: {
						'username': $scope.username
					},
					file: file
				}).progress(function (evt) {
					var progressPercentage = parseInt(100.0 * evt.loaded / evt.total);
					$scope.log = 'progress: ' + progressPercentage + '% ' + evt.config.file.name + '\n' + $scope.log;
				}).success(function (data, status, headers, config) {
					$timeout(function () {
						$scope.log = 'file: ' + config.file.name + ', Response: ' + JSON.stringify(data) + '\n' + $scope.log;
					});
				});
			}
		}
	};
}
