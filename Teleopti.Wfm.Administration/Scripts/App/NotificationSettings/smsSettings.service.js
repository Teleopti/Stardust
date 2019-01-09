(function () {
    'use strict';

	angular.module('smsSettingsModule').factory('smsSettingsService', smsSettingsService);

	smsSettingsService.$inject = ['$http'];

    function smsSettingsService($http) {
        var vm = this;

        var smsSettings = {
			get: get,
			post: post
        };

        return smsSettings;

        function get() {
			return $http.get('/smsnotificationSettings.json').then(getSmsSettings).catch(getSmsSettingsFailed);

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

		function post(data) {
			return $http(
				{
					url: '/smsnotificationSettings.json',
					method: 'POST',
					data: data
				}
			).then(postSmsSettings).catch(postSmsSettingsFailed);

			function postSmsSettings(response) {
				return {
					error: false,
					message: "Settings saved",
					data: response.data
				};
			}

			function postSmsSettingsFailed(error) {
				return {
					error: true,
					message: "Something went wrong in saving data"
				};
			}
		}
    }
})();