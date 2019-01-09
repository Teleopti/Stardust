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
			console.log(tenantId);
			var headersObj = tokenHeaderService.getHeaders();

			return $http({
				url: '/GetEmailSettingsToTenant/tenant/' + tenantId,
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

		function post(data) {
			return $http({
				url: '/smtpnotificationSettings.json',
				method: 'POST',
				data: data
			})
				.then(postEmailSettings)
				.catch(postEmailSettingsFailed);

			function postEmailSettings(response) {
				return {
					error: false,
					message: 'Settings saved',
					data: response.data
				};
			}

			function postEmailSettingsFailed(error) {
				return {
					error: true,
					message: 'Something went wrong in saving data'
				};
			}
		}
	}
})();
