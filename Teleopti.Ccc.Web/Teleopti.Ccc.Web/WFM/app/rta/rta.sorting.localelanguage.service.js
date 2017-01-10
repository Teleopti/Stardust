(function() {
	'use strict';

	angular.module('wfm.rta')
	.factory('RtaLocaleLanguageSortingService', RtaLocaleLanguageSortingService);

	RtaLocaleLanguageSortingService.$inject = ['CurrentUserInfo'];

		function RtaLocaleLanguageSortingService (CurrentUserInfo) {
			var service = {
				sort: sort
			}

			return service;
			/////////////////////

			function sort(a,b){
				if (a == null && b == null)
					return 0;
				if (a == null)
					return -1;
				if (b == null)
					return 1;
				return a.localeCompare(b,CurrentUserInfo.CurrentUserInfo().Language);
			}
		};
})();
