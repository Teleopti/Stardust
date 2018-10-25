(function() {
	'use strict';

	angular.module('currentUserInfoService').service('CurrentUserInfo', CurrentUserInfo);

	CurrentUserInfo.$inject = ['AuthenticationRequests', '$q', '$sessionStorage', 'wfmI18nService', 'Settings'];

	function CurrentUserInfo(AuthenticationRequests, $q, $sessionStorage, wfmI18nService, Settings) {
		var userName;
		var defaultTimeZone;
		var defaultTimeZoneName;
		var language;
		var dateFormatLocale;
		var timeout;
		var firstDayOfWeek;
		var dayNames;

		var isTeleoptiApplicationLogon;
		this.SetCurrentUserInfo = SetCurrentUserInfo;
		this.CurrentUserInfo = CurrentUserInfo;
		this.getCurrentUserFromServer = getCurrentUserFromServer;
		this.initContext = initContext;
		this.isConnected = isConnected;
		this.resetContext = resetContext;
		this.isTeleoptiApplicationLogon = isTeleoptiApplicationLogon;

		function SetCurrentUserInfo(data) {
			userName = data.UserName;
			defaultTimeZone = data.DefaultTimeZone;
			defaultTimeZoneName = data.DefaultTimeZoneName;
			language = data.Language;
			dateFormatLocale = data.DateFormatLocale;
			firstDayOfWeek = data.FirstDayOfWeek;
			dayNames = data.DayNames;
			isTeleoptiApplicationLogon = data.IsTeleoptiApplicationLogon;
		}

		function CurrentUserInfo() {
			return {
				UserName: userName,
				DefaultTimeZone: defaultTimeZone,
				DefaultTimeZoneName: defaultTimeZoneName,
				Language: language,
				DateFormatLocale: dateFormatLocale,
				FirstDayOfWeek: firstDayOfWeek,
				IsTeleoptiApplicationLogon: isTeleoptiApplicationLogon,
				DayNames: dayNames || []
			};
		}

		function getCurrentUserFromServer() {
			return AuthenticationRequests.getCurrentUser();
		}

		function initContext() {
			var deferred = $q.defer();
			var context = getCurrentUserFromServer();

			context.success(function(data) {
				timeout = Date.now() + 90000;
				wfmI18nService.setLocales(data);
				SetCurrentUserInfo(data);
				Settings.init();
				deferred.resolve(data);
			});
			return deferred.promise;
		}

		function isConnected() {
			return timeout > Date.now();
		}

		function resetContext() {
			if (window.location.hash.length > '#/'.length) {
				var d = new Date();
				d.setTime(d.getTime() + 5 * 60 * 1000);
				var expires = 'expires=' + d.toUTCString();
				document.cookie = 'returnHash=WFM/' + window.location.hash + '; ' + expires + '; path=/';
			}
			timeout = Date.now();
			$sessionStorage.$reset();
			window.location.href = 'Authentication?redirectUrl=' + window.location.hash;
		}
	}
})();
