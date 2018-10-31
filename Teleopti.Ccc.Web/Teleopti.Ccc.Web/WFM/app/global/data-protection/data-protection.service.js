(function () {
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

		var dataProtectionQuestionText = $resource('../api/Global/DataProtection/QuestionText', {},
		{
			query: {
				method: 'GET',
				params: {},
				isArray: false,
				transformResponse: function(data) {
					return { text: angular.fromJson(data) }
				}
			}
		});

		var service = {
			iAgree: iAgree,
			iDontAgree: iDontAgree,
			dataProtectionQuestionText: dataProtectionQuestionText
		};

		return service;
	}
})();
