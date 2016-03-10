(function() {
	'use strict';
	angular.module('wfm.notice')
		.service('NoticeService', ['$rootScope', 'growl', function($rootScope, growl) {
			var service = {};
			var message;
			service.warning = function(text, ttl, disableCountDown, beDeleted) {
				message = growl.warning(text, {
					ttl: ttl,
					disableCountDown: disableCountDown
				});
				message.beDeleted = beDeleted;
				destroyOnStateChange(message);
				return message;
			};

			 function destroyOnStateChange (message) {
				$rootScope.$on('$stateChangeSuccess', function() {
					if (message !== undefined && message.beDeleted) {
						message.destroy();
					} else {
						return;
					}
				});
			};

			return service;
		}]);
})();
