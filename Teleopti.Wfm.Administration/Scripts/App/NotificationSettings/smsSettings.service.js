(function () {
    'use strict';

	angular.module('smsSettingsModule').factory('smsSettingsService', smsSettingsService);

	smsSettingsService.$inject = ['$http'];

    function smsSettingsService($http) {
        var vm = this;

        var smsSettings = {
            get: get
        };

        return smsSettings;

        function get() {
			return $http.get('/smsnotificationSettings.json').then(getSmsSettings).catch(getSmsSettingsFailed);

            function getSmsSettings(response) {
                return response.data;
            }

            function getSmsSettingsFailed(error) {
                console.log('failed' + error.data);
            }
        }
    }
})();