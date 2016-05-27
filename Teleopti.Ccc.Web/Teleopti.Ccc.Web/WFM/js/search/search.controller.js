(function() {
	'use strict';
	angular.module('wfm.searching', ['restSearchService'])
	.controller('SearchCtrl', [
		'$scope', '$filter', '$state', 'SearchSvrc','$timeout',
		function($scope, $filter, $state, SearchSvrc, $timeout) {
			$scope.keyword = '';
			$scope.searchmode = false;
			var searchbar = document.getElementById("searchbar");

			$scope.clickedItem = function(obj) {
				$state.go(obj.Url, { id: obj.Id });
			};

			$scope.clearSearchbar = function () {
				$scope.keyword = '';
				search($scope.keyword);
			};
			$scope.resetSearch = function() {
				$scope.clearSearchbar();
				$scope.searchmode=false;
			};

			$scope.searchKeyword = function() {
				if ($scope.keyword.length > 1) {
					search($scope.keyword);
				} else {
					search('');
				}
			};

			$scope.searchbarSetFocus = function() {
				$scope.clearSearchbar();
				$scope.searchmode = !$scope.searchmode;
				$timeout(function() {
					searchbar.focus();
				}, 10);
			};

			$scope.delayFocus = function() {
				$timeout(function() {
					$scope.searchbarSetFocus();
				}, 150);
			};

			var search = function(keyword) {
				$scope.searchResultGroups = [];
				$scope.searchResult = [];
				if (keyword != '') {
					SearchSvrc.search.query({ keyword: keyword }).$promise.then(function(result) {
						$scope.searchResult = result;
						for (var i = 0; i < $scope.searchResult.length; i++) {
							var name = $scope.searchResult[i].SearchGroup;
							if ($scope.searchResultGroups.indexOf(name) == -1) $scope.searchResultGroups.push(name);
						}
					});
				}
			};
		}
	]);
})();
