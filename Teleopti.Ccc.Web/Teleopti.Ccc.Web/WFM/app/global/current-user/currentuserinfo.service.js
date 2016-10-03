(function() {
    'use strict';

    angular
        .module('currentUserInfoService')
        .service('CurrentUserInfo', CurrentUserInfo);

    CurrentUserInfo.$inject = ['AuthenticationRequests', '$q', '$sessionStorage', 'wfmI18nService', 'BusinessUnitsService', 'ThemeService', 'Settings'];

    function CurrentUserInfo(AuthenticationRequests, $q, $sessionStorage, wfmI18nService, BusinessUnitsService, ThemeService, Settings) {
				var userName;
				var defaultTimeZone;
				var language;
				var dateFormatLocale;
				var timeout;
				var theme;
        this.SetCurrentUserInfo = SetCurrentUserInfo;
				this.CurrentUserInfo = CurrentUserInfo;
				this.getCurrentUserFromServer = getCurrentUserFromServer;
				this.initContext = initContext;
				this.isConnected = isConnected;
				this.resetContext = resetContext;

        function SetCurrentUserInfo(data) {
					userName = data.UserName;
					defaultTimeZone = data.DefaultTimeZone;
					language = data.Language;
					dateFormatLocale = data.DateFormatLocale;
					theme = data.Theme;
        }

				function CurrentUserInfo() {
					return {
						UserName: userName,
						DefaultTimeZone: defaultTimeZone,
						Language: language,
						DateFormatLocale: dateFormatLocale,
						Theme: theme
					}
				}

				function getCurrentUserFromServer() {
					return AuthenticationRequests.getCurrentUser();
				}

				function initContext() {
					var deferred = $q.defer();
					var context = getCurrentUserFromServer();

					context.success(function (data) {
						timeout = Date.now() + 90000;
						wfmI18nService.setLocales(data);
						SetCurrentUserInfo(data);
						BusinessUnitsService.initBusinessUnit();
						Settings.init();
						ThemeService.init().then(function(){
							deferred.resolve(data);
						},function(error){
							deferred.reject(error);
						});
					});
					return deferred.promise;
				}

			function isConnected() {
				return timeout > Date.now();
			}

			function resetContext() {
				if (window.location.hash.length > "#/".length) {
					var d = new Date();
					d.setTime(d.getTime() + (5 * 60 * 1000));
					var expires = 'expires=' + d.toUTCString();
					document.cookie = 'returnHash=WFM/' + window.location.hash + '; ' + expires + '; path=/';
				}
				timeout = Date.now();
				$sessionStorage.$reset();
				window.location.href = 'Authentication?redirectUrl='+window.location.hash;
			};
    }
})();
