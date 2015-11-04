'use strict';

(function() {
	angular.module('wfm.pagination', []);
})();

(function () {
	var WfmPaginationCtrl = function ($scope) {
		var vm = this;
		vm.gotoPage = function (pageIndex) {
			vm.paginationOptions.pageNumber = pageIndex;
			if (vm.getDataForPageCallback !=undefined)
				vm.getDataForPageCallback(pageIndex);
		};

		vm.getVisiblePageNumbers = function (start, end) {
			var displayPageCount = 5;
			var ret = [];

			if (!start) {
				start = vm.paginationOptions.totalPages;
			}

			if (!end) {
				end = start;
				start = 1;
			}

			var leftBoundary = start;
			var rightBoundary = end;
			if (end - start >= displayPageCount) {
				var index = vm.paginationOptions.pageNumber;

				if (index < displayPageCount - 1) {
					leftBoundary = 1;
					rightBoundary = displayPageCount;
				} else if (end - index < 3) {
					leftBoundary = end - displayPageCount + 1;
					rightBoundary = end;
				} else {
					leftBoundary = index - Math.floor(displayPageCount / 2) > 1 ? index - Math.floor(displayPageCount / 2) : 1;
					rightBoundary = index + Math.floor(displayPageCount / 2) > end ? end : index + Math.floor(displayPageCount / 2);
				}
			}

			for (var i = leftBoundary; i <= rightBoundary ; i++) {
				ret.push(i);
			}

			return ret;
		};
	}
	var directive = function () {
		return {
			controller: 'WfmPaginationCtrl',
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: "js/global/wfmpagination/wfmpagination.html",
			scope: {
				paginationOptions: '=?',
				getDataForPageCallback: '=?'
			},
			link: linkFunction
		};
	};
	angular.module('wfm.pagination')
		.directive('wfmPagination', directive)
		.controller('WfmPaginationCtrl', ['$scope', WfmPaginationCtrl]);

	function linkFunction(scope, elem, attrs, vm) {

	};
}());
