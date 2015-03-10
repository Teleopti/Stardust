'use strict';

var search = angular.module('wfm.searching', []);
search.controller('SearchCtrl', [
	'$scope', '$filter', 'SearchSvrc',
	function ($scope, $filter, SearchSvrc) {
		$scope.searchResult = [];
		$scope.keyword = '';
		$scope.searchKeyword = function () {
			SearchSvrc.search.query({ keyword: $scope.keyword }).$promise.then(function(result) {
				$scope.searchResult = result;
			});
		};
	}
]);