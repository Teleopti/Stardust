﻿(function () {
	'use strict';

	angular.module('wfm.dataProtection')
        .factory('DataProtectionService', DataProtectionService);

	DataProtectionService.$inject = ['$resource'];

	function DataProtectionService($resource) {
		var iAgree = $resource('../api/Global/DataProtection/Yes', {}, {
			query: { method: 'POST', params: {}, isArray: false }
		});

		var iDontAgree = $resource('../api/Global/DataProtection/No', {}, {
			query: { method: 'POST', params: {}, isArray: false }
		});

		var service = {
			iAgree: iAgree,
			iDontAgree: iDontAgree
		};

		return service;
	}
})();
