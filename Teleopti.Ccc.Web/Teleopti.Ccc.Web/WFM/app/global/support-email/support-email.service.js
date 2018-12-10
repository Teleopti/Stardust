(function() {
	'use strict';
	angular.module('supportEmailService').factory('SupportEmailService', SupportEmailService);
	SupportEmailService.$inject = ['$http'];
	function SupportEmailService($http) {
		var service = {
			init: init
		};
		return service;
		function init() {
			return $http.get('../api/Settings/SupportEmail').success(function(data) {
				service.supportEmailSetting = data ? data : 'ServiceDesk@teleopti.com';
			});
		}
	}
})();
