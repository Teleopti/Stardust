(function() {
	'use strict';

	angular.module('wfm.rta').service('RtaLocaleLanguageSortingService',
		function() {
			var findLocaleLanguage = function(){
				if (navigator.userLanguage) // IE
					return navigator.userLanguage;
				if (navigator.language) // FF && CHROME
					return navigator.languages[0];
				return "en";
			}();

			this.sort = function(a,b){
				if (a == null && b == null)
					return 0;
				if (a == null)
					return -1;
				if (b == null)
					return 1;
				return a.localeCompare(b,findLocaleLanguage);
			}
		}
	);
})();