(function () {
	'use strict';

	angular
		.module('localeLanguageSortingService', ['currentUserInfoService'])
		.factory('localeLanguageSortingService', localeLanguageSortingService);

	localeLanguageSortingService.$inject = ['CurrentUserInfo'];

	function localeLanguageSortingService(CurrentUserInfo) {
		var service = {
			sort: sort,
			loopSort: loopSort,
			localeSort: localeSort
		}

		return service;
		/////////////////////

		function sort(a, b) {
			if (a == null && b == null)
				return 0;
			if (a == null)
				return -1;
			if (b == null)
				return 1;
			return a.localeCompare(b, CurrentUserInfo.CurrentUserInfo().Language);
		}

		// for tree structure data, single key only!
		// use: array = localeLanguageSortingService.loopSort(array, 'ChildFunctions', '+FilterType', true);
		function loopSort(arr, childType, key, loopParent) {
			if (!key) return;
			if (angular.isUndefined(loopParent) || loopParent) {
				arr.sort(localeSort(key));
			}
			arr.forEach(function (item) {
				if (item[childType] && item[childType].length > 0) {
					var children = item[childType];
					loopSort(children, childType, key, true);
				}
			});
			return arr;
		}

		// for object arrays, multi key allow!
		// use: array.sort(localeLanguageSortingService.localeSort('+FilterType','-Name'));
		function localeSort() {
			var _args = Array.prototype.slice.call(arguments);
			return function (a, b) {
				for (var x in _args) {

					var ax = a[_args[x].substring(1)];
					var bx = b[_args[x].substring(1)];
					var cx;

					if (angular.isUndefined(ax) || angular.isUndefined(bx))
						return;

					ax = (typeof ax == "string" || false) ? ax.toLowerCase() : ax / 1;
					bx = (typeof ax == "string" || false) ? bx.toLowerCase() : bx / 1;

					if (_args[x].substring(0, 1) === "-") { cx = ax; ax = bx; bx = cx; }
					if (ax !== bx && (typeof ax == "string" || false)) {
						return ax.localeCompare(bx, CurrentUserInfo.CurrentUserInfo().Language);
					} else if (ax !== bx){
						return ax < bx ? -1 : 1;
					} else {
						return;
					}
				}
			}
		}
	};
})();
