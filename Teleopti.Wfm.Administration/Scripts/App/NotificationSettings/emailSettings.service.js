(function () {
    'use strict';

    angular.module('emailSettingsModule').factory('emailSettingsService', emailSettingsService);

    emailSettingsService.$inject = ['$http'];

    function emailSettingsService($http) {
        var vm = this;

        var emailSettings = {
			get: get,
			post: post
        };

        return emailSettings;

        function get() {
            return $http.get('/smtpnotificationSettings.json').then(getEmailSettings).catch(getEmailSettingsFailed);

            function getEmailSettings(response) {
                return response.data;
            }

            function getEmailSettingsFailed(error) {
                return {
					error: true,
					message : "Something went wrong in loading data"
                };
            }
		}

		function post(data) {
			return $http(
				{
					url: '/smtpnotificationSettings.json',
					method: 'POST',
					data: data
				}
			).then(postEmailSettings).catch(postEmailSettingsFailed);

			function postEmailSettings(response) {
				return {
					error: false,
					message: "Settings saved",
					data: response.data
				};
			}

			function postEmailSettingsFailed(error) {
				return {
					error: true,
					message: "Something went wrong in saving data"
				};
			}
		}
    }
})();