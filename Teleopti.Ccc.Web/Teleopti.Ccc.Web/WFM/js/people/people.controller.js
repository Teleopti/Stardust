'use strict';

angular
	.module('wfm.people', ['peopleService', 'peopleSearchService'])
	.constant('chunkSize', 50)
	.controller('PeopleCtrl', [
		'$scope', '$filter', '$state', 'PeopleSearch', PeopleController
	]);

function PeopleController($scope, $filter, $state, SearchSvrc) {
	$scope.searchResult = [];
	$scope.pageSize = 20;
	$scope.keyword = '';
	$scope.totalPages = 0;
	$scope.currentPageIndex = 1;
	$scope.searchKeywordChanged = false;
	$scope.advancedSearchForm = {};
	$scope.searchCriteriaDic = {};

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
		if ($scope.searchKeywordChanged) {
			$scope.currentPageIndex = 1;
		}
		SearchSvrc.search.query({
			keyword: $scope.keyword,
			pageSize: $scope.pageSize,
			currentPageIndex: $scope.currentPageIndex
		}).$promise.then(function (result) {
			$scope.searchResult = result.People;
			$scope.optionalColumns = result.OptionalColumns;
			$scope.totalPages = result.TotalPages;
			$scope.keyword = $scope.defautKeyword();
			$scope.searchKeywordChanged = false;
		});
	};

	$scope.defautKeyword = function () {
		if ($scope.keyword == '' && $scope.searchResult != undefined && $scope.searchResult.length > 0) {
			return "\"" + $scope.searchResult[0].Team.replace("/", "\" \"") + "\"";
		}
		return $scope.keyword;
	};

	$scope.range = function (start, end) {
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
	$scope.toggleAdvancedSearchOption = function () {
		$scope.showAdvancedSearchOption = !$scope.showAdvancedSearchOption;
		//event.stopPropagation();
	};

	//window.onclick = function () {
	//	var isChild = $element.find(event.target).length > 0;
	//	if ($scope.showAdvancedSearchOption && !isChild) {
	//		$scope.showAdvancedSearchOption = false;
	//		$scope.$apply();
	//	}
	//};
 
	function getSearchCriteria(title, value) {
		return value != undefined && value != "" ? title + ": " + value + ", " : "";
	}

	$scope.advancedSearch = function () {
		$scope.showAdvancedSearchOption = false;
		//triger search
		if ($scope.searchKeywordChanged) {
			$scope.currentPageIndex = 1;
		}
		var keyword = "";
		angular.forEach(allSearchTypes, function (searchType) {
			// Change first letter to lowercase
			var title = searchType.charAt(0).toLowerCase() + searchType.slice(1);
			keyword += getSearchCriteria(title, $scope.advancedSearchForm[searchType]);
		});

		if (keyword != "") {
			keyword = keyword.substring(0, keyword.length - 2);
		}

		$scope.keyword = keyword;

		SearchSvrc.searchWithOption.query(JSON.stringify({
			SearchCriteria: $scope.advancedSearchForm,
			PageSize: $scope.pageSize,
			CurrentPageIndex: $scope.currentPageIndex
		})).$promise.then(function (result) {
			$scope.searchResult = result.People;
			$scope.optionalColumns = result.OptionalColumns;
			$scope.totalPages = result.TotalPages;
			$scope.keyword = $scope.defautKeyword();
			$scope.searchKeywordChanged = false;
		});
	}

	SearchSvrc.isAdvancedSearchEnabled.query({ toggle: 'WfmPeople_AdvancedSearch_32973' }).$promise.then(function (result) {
		$scope.isAdvancedSearchEnabled = result.IsEnabled;
	});

	$scope.searchKeyword();
}
