(function() {
	'use strict';

	angular.module('emailSettingsModule').factory('emailSettingsService', emailSettingsService);

	emailSettingsService.$inject = ['$http', 'tokenHeaderService'];

	function emailSettingsService($http, tokenHeaderService) {
		var vm = this;

		var emailSettings = {
			get: get,
			post: post
		};

		return emailSettings;

		function get(tenantId) {
			var headersObj = tokenHeaderService.getHeaders();

			return $http({
				url: '/GetEmailSettings/tenant/' + tenantId,
				method: 'GET',
				headers: headersObj.headers
			})
				.then(getEmailSettings)
				.catch(getEmailSettingsFailed);

			function getEmailSettings(response) {
				return response.data;
			}

			function getEmailSettingsFailed(error) {
				return {
					error: true,
					message: 'Something went wrong in loading data'
				};
			}
		}

		function post(tenantId, data) {
			var headersObj = tokenHeaderService.getHeaders();

			var postData = {
				TenantId: tenantId,
				Settings: data
			}

			return $http({
				url: '/AddEmailSettings',
				method: 'POST',
				data: postData, 
				headers: headersObj.headers
			})
				.then(postEmailSettings)
				.catch(postEmailSettingsFailed);

			function postEmailSettings(response) {
				return response;
			}

			function postEmailSettingsFailed(response) {
				return response;
			}
		}
	}
})();
