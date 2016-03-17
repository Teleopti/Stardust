(function() {
	'use strict';
	angular.module('wfm.notice')
		.service('NoticeService', function($rootScope) {
			var service = {};
			service.notice = {};

			service.addNotice = function(content, timeToLive, destroyOnStateChange) {
				service.notice = {
					content: content,
					timeToLive: timeToLive,
					destroyOnStateChange: destroyOnStateChange
				};
				return service.notice;
			};

			return service;
		});
})();
