(function () {
    'use strict';

	angular.module('smsSettingsModule').factory('smsSettingsService', smsSettingsService);

	smsSettingsService.$inject = ['$http', 'tokenHeaderService'];

	function smsSettingsService($http, tokenHeaderService) {
        var vm = this;

        var smsSettings = {
			get: get,
			post: post
        };

        return smsSettings;

		function get(tenantId) {
			var headersObj = tokenHeaderService.getHeaders();
			return $http({
				url: '/GetSmsSettings/tenant/' + tenantId,
				method: 'GET',
				headers: headersObj.headers
			})
				.then(getSmsSettings)
				.catch(getSmsSettingsFailed);

            function getSmsSettings(response) {
                return response.data;
            }

            function getSmsSettingsFailed(error) {
				return {
					error: true,
					message: "Something went wrong in loading data"
				};
            }
		}

		function post(tenantId, data) {
			var headersObj = tokenHeaderService.getHeaders();

			var postData = {
				TenantId: tenantId,
				Settings: data
			}

			return $http(
				{
					url: '/AddSmsSettings',
					method: 'POST',
					data: postData,
					headers: headersObj.headers
				})
				.then(postSmsSettings)
				.catch(postSmsSettingsFailed);

			function postSmsSettings(response) {
				return response;
			}

			function postSmsSettingsFailed(error) {
				return response;
			}
		}
    }
})();