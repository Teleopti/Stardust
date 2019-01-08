(function () {
    'use strict';

    angular.module('emailSettingsModule').factory('emailSettingsService', emailSettingsService);

    emailSettingsService.$inject = ['$http'];

    function emailSettingsService($http) {
        var vm = this;

        var emailSettings = {
            get: get
        };

        return emailSettings;

        function get() {
            return $http.get('/notificationSettings.json').then(getEmailSettings).catch(getEmailSettingsFailed);

            function getEmailSettings(response) {
                return response.data;
            }

            function getEmailSettingsFailed(error) {
                console.log('failed' + error.data);
            }
        }
    }
})();