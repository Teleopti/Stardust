(function() {
	'use strict';

	angular.module('wfm.rta').service('RtaLocaleLanguageSortingService',['CurrentUserInfo',
		function(CurrentUserInfo) {
			this.sort = function(a,b){
				if (a == null && b == null)
					return 0;
				if (a == null)
					return -1;
				if (b == null)
					return 1;
				return a.localeCompare(b,CurrentUserInfo.CurrentUserInfo().Language);
			}
		}
	]);
})();