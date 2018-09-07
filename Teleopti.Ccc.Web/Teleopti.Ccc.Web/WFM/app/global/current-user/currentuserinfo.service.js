(function() {
	'use strict';

	angular.module('currentUserInfoService').service('CurrentUserInfo', CurrentUserInfo);

	CurrentUserInfo.$inject = [
		'AuthenticationRequests',
		'$q',
		'$sessionStorage',
		'wfmI18nService',
		'BusinessUnitsService',
		'Settings'
	];

	function CurrentUserInfo(
		AuthenticationRequests,
		$q,
		$sessionStorage,
		wfmI18nService,
		BusinessUnitsService,
		Settings
	) {
		var userName;
		var defaultTimeZone;
		var defaultTimeZoneName;
		var language;
		var dateFormatLocale;
		var timeout;
		var theme;
		var firstDayOfWeek;
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
			theme = data.Theme;
			firstDayOfWeek = data.FirstDayOfWeek;
			isTeleoptiApplicationLogon = data.IsTeleoptiApplicationLogon;
		}

		function CurrentUserInfo() {
			return {
				UserName: userName,
				DefaultTimeZone: defaultTimeZone,
				DefaultTimeZoneName: defaultTimeZoneName,
				Language: language,
				DateFormatLocale: dateFormatLocale,
				Theme: theme,
				FirstDayOfWeek: firstDayOfWeek,
				IsTeleoptiApplicationLogon: isTeleoptiApplicationLogon
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
				BusinessUnitsService.initBusinessUnit();
				Settings.init();
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
