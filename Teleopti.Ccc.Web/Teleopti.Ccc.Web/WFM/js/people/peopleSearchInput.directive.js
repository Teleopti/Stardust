'use strict';

(function () {
	angular.module('wfm.people').run([
		'$templateCache', function ($templateCache) {
			$templateCache.put('wfm-people-search-input-default.tpl.html',
				'<label for="simple-people-search" class="input-prepend people-search">' +
					'<i class="mdi mdi-magnify">' +
					'<md-tooltip>Filter people</md-tooltip></i>' +
					'<input id="simple-people-search" type="text" placeholder="{{\'Search\'|translate}}..." '+
						'ng-model="vm.searchOptions.keyword" ng-keydown="$event.which === 13 && vm.searchCallback(vm.searchOptions.keyword) || vm.turnOffAdvancedSearch()" ng-change="vm.validateSearchKeywordChanged()" ' +
						'ng-click="vm.toggleAdvancedSearchOption($event)" />'+
					'<advance-search class="dropdown" ng-cloak ng-if="vm.searchOptions.isAdvancedSearchEnabled && vm.showAdvancedSearchOption" outside-click="vm.turnOffAdvancedSearch()"></advance-search>' +
				'</label>'
			);
		}]);
})();

(function () {
	var PeopleSearchInputCtrl = function ($scope) {
		var vm = this;
		vm.advancedSearchForm = {};
		vm.searchCriteriaDic = {};

		var searchExpressionSeprator = ";";
		var keyValueSeprator = ":";

		vm.validateSearchKeywordChanged = function () {
			vm.searchOptions.searchKeywordChanged = true;
			parseSearchExpressionInputted();
		};
		vm.turnOffAdvancedSearch = function () {
			vm.showAdvancedSearchOption = false;
		};

		vm.toggleAdvancedSearchOption = function ($event) {
			vm.showAdvancedSearchOption = !vm.showAdvancedSearchOption;
			$event.stopPropagation();
			parseSearchExpressionInputted();
		};
		vm.advancedSearch = function () {
			vm.showAdvancedSearchOption = false;

			var expression = "";
			angular.forEach(allSearchTypes, function (searchType) {
				// Change first letter to lowercase
				var title = searchType.charAt(0).toLowerCase() + searchType.slice(1);
				expression += getSearchCriteria(title, vm.advancedSearchForm[searchType]);
			});

			expression = expression.trim();
			if (expression !== "" && expression !== vm.searchOptions.keyword) {
				vm.searchOptions.searchKeywordChanged = true;
			}
			vm.searchOptions.keyword = expression;
			if (vm.searchCallback)
				vm.searchCallback(expression);
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

		function parseSearchValue(value) {
			if (value === undefined || value === null || value.trim().length === 0) {
				return '';
			}
			var displayValue = value.trim();

			var quotedKeywords = "";
			var pattern = /['"](.*?)['"]/ig;
			var match;
			while (match = pattern.exec(displayValue)) {
				var keyword = match[1].trim();
				if (keyword.length > 0) {
					quotedKeywords = quotedKeywords + ' "' + keyword + '"';
				}
			}
			quotedKeywords = quotedKeywords.trim();

			var unquotedKeywords = displayValue
				.replace(pattern, '').trim()
				.replace('"', '').trim();

			return (quotedKeywords + ' ' + unquotedKeywords).trim();
		}

		function parseSearchExpressionInputted() {
			vm.advancedSearchForm = {};
			if (vm.searchOptions.keyword.indexOf(keyValueSeprator) !== -1) {
				var expression = vm.searchOptions.keyword.trim();
				if (expression.charAt(expression.length - 1) !== searchExpressionSeprator) {
					expression = expression + searchExpressionSeprator;
				}
				var regex = /(\S*?):\s{0,}(.*?);/ig;
				var match;
				while (match = regex.exec(expression)) {
					var searchType = match[1].trim();
					var searchValue = parseSearchValue(match[2].trim());
					setSearchFormProperty(searchType, searchValue);
				}
			}
		}

		function getSearchCriteria(title, value) {
			var keyWords = parseSearchValue(value);
			return keyWords.length > 0
				? title + keyValueSeprator + ' ' + keyWords + searchExpressionSeprator + ' '
				: '';
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