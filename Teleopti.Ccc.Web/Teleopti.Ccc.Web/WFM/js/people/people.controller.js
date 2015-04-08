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
		});

	function PeopleController($scope, $filter, $state, SearchSvrc) {
	    $scope.searchResult = [];
	    $scope.pageSize = 20;
	    $scope.keyword = '';
	    $scope.totalPages = 0;
	    $scope.currentPageIndex = 1;
		$scope.searchKeyword = function () {
		    SearchSvrc.search.query({ keyword: $scope.keyword, pageSize: $scope.pageSize, currentPageIndex: $scope.currentPageIndex }).$promise.then(function (result) {
		        $scope.searchResult = result.People;
		        $scope.totalPages = result.TotalPages;
			});
		};

		$scope.range = function (start, end) {
		    var ret = [];
		    if (!end) {
		        end = start;
		        start = 1;
		    }
		    for (var i = start; i < end; i++) {
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

})();