(function(angular) {

	'use strict';

	angular.module('wfm.teamSchedule').service('guidgenerator', function() {
		
		function s4() {
			return Math.floor((1 + Math.random()) * 0x10000)
				.toString(16)
				.substring(1);
		}

		this.newGuid = function() {
			return s4() + s4() + '-' + s4() + '-' + s4() + '-' + s4() + '-' + s4() + s4() + s4();
		};

	});

})(window.angular);