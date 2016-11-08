(function() {
	'use strict';
	angular
		.module('wfm.utilities')
		.service('guidgenerator', guidgenerator);

	function guidgenerator() {
		
		function s4() {
			return Math.floor((1 + Math.random()) * 0x10000)
				.toString(16)
				.substring(1);
		}

		var newGuid = function() {
			return s4() + s4() + '-' + s4() + '-' + s4() + '-' + s4() + '-' + s4() + s4() + s4();
		};

		return {
			newGuid : newGuid
		};
	}
})();