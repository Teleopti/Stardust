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


	$scope.validateSearchKeywordChanged = function () {
		$scope.searchKeywordChanged = true;
		$scope.advancedSearchForm = {};
		if ($scope.keyword.indexOf(':') != -1) {
			var searchTerms = $scope.keyword.split(',');
			angular.forEach(searchTerms, function (searchTerm) {
				var termSplitter = searchTerm.indexOf(':');
				var type = searchTerm.substring(0, termSplitter).trim();
				var value = searchTerm.substring(termSplitter + 1, searchTerm.length).trim();
				if (type.toUpperCase() == "FIRSTNAME") {
					$scope.advancedSearchForm.FirstName = value;
				}
				if (type.toUpperCase() == "LASTNAME") {
					$scope.advancedSearchForm.LastName = value;
				}
				if (type.toUpperCase() == "EMPLOYMENTNUMBER") {
					$scope.advancedSearchForm.EmploymentNumber = value;
				}
				if (type.toUpperCase() == "ORGANIZATION") {
					$scope.advancedSearchForm.Organization = value;
				}
				if (type.toUpperCase() == "ROLE") {
					$scope.advancedSearchForm.Role = value;
				}
				if (type.toUpperCase() == "CONTRACT") {
					$scope.advancedSearchForm.Contract = value;
				}
				if (type.toUpperCase() == "CONTRACTSCHEDULE") {
					$scope.advancedSearchForm.ContractSchedule = value;
				}
				if (type.toUpperCase() == "SHIFTBAG") {
					$scope.advancedSearchForm.ShiftBag = value;
				}
				if (type.toUpperCase() == "PARTTIMEPERCENTAGE") {
					$scope.advancedSearchForm.PartTimePercentage = value;
				}
				if (type.toUpperCase() == "SKILL") {
					$scope.advancedSearchForm.Skill = value;
				}
				if (type.toUpperCase() == "BUDGETGROUP") {
					$scope.advancedSearchForm.BudgetGroup = value;
				}
				if (type.toUpperCase() == "NOTE") {
					$scope.advancedSearchForm.Note = value;
				}
			});
		}
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
			return $scope.searchResult[0].Team;
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
	}


	$scope.advancedSearch = function () {
		$scope.showAdvancedSearchOption = false;
		//triger search
		if ($scope.searchKeywordChanged) {
			$scope.currentPageIndex = 1;
		}
		var keyword = "";
		if (!IsUndefinedOrEmpty($scope.advancedSearchForm.FirstName)) {
			keyword += "firstName: " + $scope.advancedSearchForm.FirstName + ", ";
		}
		if (!IsUndefinedOrEmpty($scope.advancedSearchForm.LastName)) {
			keyword += "lastName: " + $scope.advancedSearchForm.LastName + ", ";
		}
		if (!IsUndefinedOrEmpty($scope.advancedSearchForm.EmploymentNumber)) {
			keyword += "employmentNumber: " + $scope.advancedSearchForm.EmploymentNumber + ", ";
		}
		if (!IsUndefinedOrEmpty($scope.advancedSearchForm.Organization)) {
			keyword += "organization: " + $scope.advancedSearchForm.Organization + ", ";
		}
		if (!IsUndefinedOrEmpty($scope.advancedSearchForm.Role)) {
			keyword += "role: " + $scope.advancedSearchForm.Role + ", ";
		}
		if (!IsUndefinedOrEmpty($scope.advancedSearchForm.Contract)) {
			keyword += "contract: " + $scope.advancedSearchForm.Contract + ", ";
		}
		if (!IsUndefinedOrEmpty($scope.advancedSearchForm.ContractSchedule)) {
			keyword += "contractSchedule: " + $scope.advancedSearchForm.ContractSchedule + ", ";
		}
		if (!IsUndefinedOrEmpty($scope.advancedSearchForm.ShiftBag)) {
			keyword += "shiftBag: " + $scope.advancedSearchForm.ShiftBag + ", ";
		}
		if (!IsUndefinedOrEmpty($scope.advancedSearchForm.PartTimePercentage)) {
			keyword += "partTimePercentage: " + $scope.advancedSearchForm.PartTimePercentage + ", ";
		}
		if (!IsUndefinedOrEmpty($scope.advancedSearchForm.Skill)) {
			keyword +=  "skill: " + $scope.advancedSearchForm.Skill + ", ";
		}
		if (!IsUndefinedOrEmpty($scope.advancedSearchForm.BudgetGroup)) {
			keyword +=  "budgetGroup: " + $scope.advancedSearchForm.BudgetGroup + ", ";
		}
		if (!IsUndefinedOrEmpty($scope.advancedSearchForm.Note)) {
			keyword += "note: " + $scope.advancedSearchForm.Note + ", ";
		}

		if (keyword != "") {
			keyword = keyword.substring(0, keyword.length - 2);
		}

		$scope.keyword = keyword;
		
		SearchSvrc.searchWithOption.query(JSON.stringify({ SearchCriteria: $scope.advancedSearchForm, PageSize: $scope.pageSize, CurrentPageIndex: $scope.currentPageIndex }))
			.$promise.then(function (result) {
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
function IsUndefinedOrEmpty(value) {
	return value == undefined || value == "";
};