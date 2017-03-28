(function() {
	'use strict';

	angular
		.module('localeLanguageSortingService', ['currentUserInfoService'])
		.factory('localeLanguageSortingService', localeLanguageSortingService);

	localeLanguageSortingService.$inject = ['CurrentUserInfo'];

	function localeLanguageSortingService(CurrentUserInfo) {
		var service = {
			sort: sort
		}

		return service;
		/////////////////////

		function sort(a, b) {
			if ((a == null || !angular.isDefined(a.value)) && (b == null || !angular.isDefined(b.value)))
				return 0;
			if (a == null || !angular.isDefined(a.value))
				return -1;
			if (b == null || !angular.isDefined(b.value))
				return 1;
			return a.value.localeCompare(b.value, CurrentUserInfo.CurrentUserInfo().Language);
		}
	};
})();
