(function () {
	'use strict';


    angular.module('wfm.people', ['peopleService', 'peopleSearchService'])
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
					scope.hideModal = function (){ 
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
		})
		.directive('whenScrollEnds', function () {
			return {
				restrict: "A",
				link: function (scope, element, attrs) {
					var visibleHeight = element.height();
					var threshold = 100;
					console.log("visibleHeight", visibleHeight);
					element.scroll(function () {
						var scrollableHeight = element.prop('scrollHeight');
						var hiddenContentHeight = scrollableHeight - visibleHeight;
						console.log("scrollableHeight", scrollableHeight);
						console.log("hiddenContentHeight", hiddenContentHeight);
					    console.log("element.scrollTop()", element.scrollTop());
					    console.log("hiddenContentHeight - element.scrollTop()", hiddenContentHeight - element.scrollTop());
						if (hiddenContentHeight - element.scrollTop() <= threshold) {
							// Scroll is almost at the bottom. Loading more rows
							scope.$apply(attrs.whenScrollEnds);
						}
					});
				}
			};
		});

	function PeopleController($scope, $filter, $state, SearchSvrc) {
	    $scope.searchResult = [];
	    $scope.pageSize = 5;
		$scope.keyword = '';
		$scope.searchKeyword = function () {
		    SearchSvrc.search.query({ keyword: $scope.keyword, pageSize: $scope.pageSize, currentPageIndex: ($scope.searchResult.length / $scope.pageSize + 1) }).$promise.then(function (result) {
				$scope.searchResult = result;
				console.log($scope.searchResult);
			});
		};

	    $scope.searchKeyword();

		//$scope.modalShown = false;
		//$scope.toggleModal = function () {
		//	$scope.modalShown = !$scope.modalShown;
		//};
	}

})();