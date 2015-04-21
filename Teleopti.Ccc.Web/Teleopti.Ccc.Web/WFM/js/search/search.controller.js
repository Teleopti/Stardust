(function() {
	'use strict';
	angular.module('wfm.searching', [])
		.controller('SearchCtrl', [
			'$scope', '$filter', '$state', 'SearchSvrc',
			function($scope, $filter, $state, SearchSvrc) {
				$scope.keyword = '';
				$scope.ResetSearch = function() {
					$scope.keyword = '';
					search($scope.keyword);
				};
				$scope.searchKeyword = function() {
					if ($scope.keyword.length > 1) {
						search($scope.keyword);
					} else {
						search('');
					}
				};
				var search = function(keyword) {
					$scope.searchResultGroups = [];
					$scope.searchResult = [];
					if (keyword != '') {
						SearchSvrc.search.query({ keyword: keyword }).$promise.then(function(result) {
							console.log(result);
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