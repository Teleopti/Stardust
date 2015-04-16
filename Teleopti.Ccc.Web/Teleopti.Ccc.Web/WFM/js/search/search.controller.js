'use strict';

var search = angular.module('wfm.searching', []);
search.controller('SearchCtrl', [
	'$scope', '$filter', '$state', 'SearchSvrc',
	function($scope, $filter, $state, SearchSvrc) {
		$scope.searchResult = [];
		$scope.keyword = '';
		$scope.searchResultGroups = [];
		$scope.searchKeyword = function() {
			SearchSvrc.search.query({ keyword: $scope.keyword }).$promise.then(function(result) {
				$scope.searchResult = result;
				for (var i = 0; i < $scope.searchResult.length; i++) {
					var name = $scope.searchResult[i].SearchGroup;
					if ($scope.searchResultGroups.indexOf(name) == -1) $scope.searchResultGroups.push(name);
				}
			});
		};
	}
]);