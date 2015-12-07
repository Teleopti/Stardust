'use strict';

(function () {
	angular.module('wfm.people').run([
		'$templateCache', function ($templateCache) {
			$templateCache.put('wfm-people-search-input-default.tpl.html',
				'<div class="input-prepend people-search">'+
					'<i class="mdi mdi-magnify"></i>' +
					'<input id="simple-search" style="width: 90%; height: 30px;" type="text" placeholder="{{\'Search\'|translate}}..." '+
						'ng-model="vm.searchOptions.keyword" ng-keydown="$event.which === 13 && vm.searchCallback() || vm.turnOffAdvancedSearch()" ng-change="vm.validateSearchKeywordChanged()" ' +
						'ng-click="vm.toggleAdvancedSearchOption($event)" />'+
					'<advance-search class="dropdown" ng-cloak ng-if="vm.searchOptions.isAdvancedSearchEnabled && vm.showAdvancedSearchOption" outside-click="vm.turnOffAdvancedSearch()"></advance-search>' +
				'</div>'
			);
		}]);
})();

(function () {
	var PeopleSearchInputCtrl = function ($scope) {
		var vm = this;
		vm.advancedSearchForm = {};
		vm.searchCriteriaDic = {};

		vm.validateSearchKeywordChanged = function () {
			vm.searchOptions.searchKeywordChanged = true;
			parseSearchKeywordInputted();
		};
		vm.turnOffAdvancedSearch = function () {
			vm.showAdvancedSearchOption = false;
		};

		vm.toggleAdvancedSearchOption = function ($event) {
			vm.showAdvancedSearchOption = !vm.showAdvancedSearchOption;
			$event.stopPropagation();
			parseSearchKeywordInputted();
		};
		vm.advancedSearch = function () {
			vm.showAdvancedSearchOption = false;

			var keyword = "";
			angular.forEach(allSearchTypes, function (searchType) {
				// Change first letter to lowercase
				var title = searchType.charAt(0).toLowerCase() + searchType.slice(1);
				keyword += getSearchCriteria(title, vm.advancedSearchForm[searchType]);
			});

			if (keyword !== "") {
				keyword = keyword.substring(0, keyword.length - 2);
			}
			if (keyword !== "" && keyword !== vm.searchOptions.keyword) {
				vm.searchOptions.searchKeywordChanged = true;
			}
			vm.searchOptions.keyword = keyword;
			if (vm.searchCallback)
				vm.searchCallback();
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
				if (propName.toUpperCase() === searchType.toUpperCase()) {
					vm.advancedSearchForm[propName] = searchValue;
				}
			});
		}

		function parseSearchKeywordInputted() {
			vm.advancedSearchForm = {};
			if (vm.searchOptions.keyword.indexOf(':') !== -1) {
				var searchTerms = vm.searchOptions.keyword.split(',');
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

		function getSearchCriteria(title, value) {
			return value !== undefined && value !== "" ? title + ": " + value + ", " : "";
		}
	}
	var directive = function () {
		return {
			controller: 'PeopleSearchInputCtrl',
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: "wfm-people-search-input-default.tpl.html",
			scope: {
				searchOptions: '=?',
				searchCallback: '=?'
			},
			link: linkFunction
		};
	};
	angular.module('wfm.people')
		.directive('peopleSearchInput', directive)
		.controller('PeopleSearchInputCtrl', ['$scope', PeopleSearchInputCtrl]);

	function linkFunction(scope, elem, attrs, vm) {

	};
}());

(function () {
	var directive = function () {
		return {
			controller: 'PeopleSearchInputCtrl',
			templateUrl: "js/people/html/advancedPeopleSearch.html"
		};
	};
	angular.module('wfm.people').directive('advanceSearch', directive);
}());