(function () {
    'use strict';

    angular.module('currentUserInfoService').service('CurrentUserInfo', CurrentUserInfo);

    CurrentUserInfo.$inject = ['$http', '$q', 'wfmI18nService'];

    function CurrentUserInfo($http, $q, wfmI18nService) {
        var timeout;
        this.SetCurrentUserInfo = SetCurrentUserInfo;
        this.CurrentUserInfo = CurrentUserInfo;
        this.Load = Load;
        this.getCurrentUserFromServer = getCurrentUserFromServer;
        this.initContext = initContext;
        this.isConnected = isConnected;

        var loaded;
        var loadedData;

        function Load() {
            if (!loaded)
                return initContext();
            return loaded;
        }

        function initContext() {
            loaded = getCurrentUserFromServer()
                .then(function (response) {
                    var data = response.data;
                    timeout = Date.now() + 90000;
                    wfmI18nService.setLocales(data);
                    return data;
                }).then(function (data) {
                    loadedData = build(data);
                    return loadedData;
                });
            return loaded;
        }

        function getCurrentUserFromServer() {
            return $http.get('../api/Global/User/CurrentUser');
        }

        function build(data) {
            return {
                UserName: data.UserName,
                DefaultTimeZone: data.DefaultTimeZone,
                DefaultTimeZoneName: data.DefaultTimeZoneName,
                Language: data.Language,
                DateFormatLocale: data.DateFormatLocale,
                FirstDayOfWeek: data.FirstDayOfWeek,
                IsTeleoptiApplicationLogon: data.IsTeleoptiApplicationLogon,
                DayNames: data.DayNames || [],
                DateTimeFormat: getDateTimeFormat(data)
            };

            function getDateTimeFormat(data) {
                if (!data.DateTimeFormat) {
                    return {};
                }
                var patternArrays = data.DateTimeFormat.ShortTimePattern.split(' ');
                var showMeridian = patternArrays.length > 1;
                var shortTimePattern = showMeridian ? patternArrays[0] + ' A' : data.DateTimeFormat.ShortTimePattern;
                return {
                    ShortTimePattern: shortTimePattern,
                    AMDesignator: data.DateTimeFormat.AMDesignator,
                    PMDesignator: data.DateTimeFormat.PMDesignator,
                    ShowMeridian: showMeridian
                };
            }
        }

        function CurrentUserInfo() {
            return loadedData;
        }

        function SetCurrentUserInfo(data) {
            loadedData = build(data);
        }

        function isConnected() {
            return timeout > Date.now();
        }
    }
})();
