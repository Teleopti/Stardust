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
			if (a == null && b == null)
				return 0;
			if (a == null)
				return -1;
			if (b == null)
				return 1;
			return a.value.localeCompare(b.value, CurrentUserInfo.CurrentUserInfo().Language);
		}
	};
})();
