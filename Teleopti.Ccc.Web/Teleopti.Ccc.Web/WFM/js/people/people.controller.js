﻿'use strict';

angular
	.module('wfm.people', ['peopleService', 'peopleSearchService'])
	.constant('chunkSize', 50)
	.controller('PeopleCtrl', [
		'$scope', '$filter', '$state', 'PeopleSearch', PeopleController
	])
	.directive('modalDialog', function () {
		return {
			restrict: 'E',
			scope: {
				show: '='
			},
			replace: true, // Replace with the template below
			transclude: true, // we want to insert custom content inside the directive
			link: function (scope, element, attrs) {
				scope.dialogStyle = {};
				if (attrs.width)
					scope.dialogStyle.width = attrs.width;
				if (attrs.height)
					scope.dialogStyle.height = attrs.height;
				scope.hideModal = function () {
					scope.show = false;
				};
			},
			template: "<div class='ng-modal' ng-show='show'>" +
				"<div class='ng-modal-overlay' ng-click='hideModal()'>" +
				"</div>" +
				"<div class='ng-modal-dialog' ng-style='dialogStyle'>" +
				"<div class='ng-modal-close' ng-click='hideModal()'>x</div>" +
				"<div class='ng-modal-dialog-content' ng-transclude></div>" +
				"</div>" +
				"</div>"
		};
	});

function PeopleController($scope, $filter, $state, SearchSvrc) {
	$scope.searchResult = [];
	$scope.pageSize = 20;
	$scope.keyword = '';
	$scope.totalPages = 0;
	$scope.currentPageIndex = 1;
	$scope.searchKeywordChanged = false;

	$scope.validateSearchKeywordChanged = function () {
		$scope.searchKeywordChanged = true;
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

	$scope.defautKeyword = function() {
		if ($scope.keyword == '' && $scope.searchResult.length > 0) {
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
		if (end - start > displayPageCount) {
			var currentPageIndex = $scope.currentPageIndex;
			leftBoundary = currentPageIndex - Math.floor(displayPageCount / 2) > 1 ? currentPageIndex - Math.floor(displayPageCount / 2) : 1;
			rightBoundary = currentPageIndex + Math.floor(displayPageCount / 2) > end ? end : currentPageIndex + Math.floor(displayPageCount / 2);
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

	$scope.searchKeyword();
}
