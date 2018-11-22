(function() {
	'use strict';

	angular.module('currentUserInfoService').service('CurrentUserInfo', CurrentUserInfo);

	CurrentUserInfo.$inject = ['AuthenticationRequests', '$q', 'wfmI18nService', 'Settings'];

	function CurrentUserInfo(AuthenticationRequests, $q, wfmI18nService, Settings) {
		var userName;
		var defaultTimeZone;
		var defaultTimeZoneName;
		var language;
		var dateFormatLocale;
		var timeout;
		var firstDayOfWeek, dayNames, dateTimeFormat;

		var isTeleoptiApplicationLogon;
		this.SetCurrentUserInfo = SetCurrentUserInfo;
		this.CurrentUserInfo = CurrentUserInfo;
		this.getCurrentUserFromServer = getCurrentUserFromServer;
		this.initContext = initContext;
		this.isConnected = isConnected;
		this.isTeleoptiApplicationLogon = isTeleoptiApplicationLogon;

		function SetCurrentUserInfo(data) {
			userName = data.UserName;
			defaultTimeZone = data.DefaultTimeZone;
			defaultTimeZoneName = data.DefaultTimeZoneName;
			language = data.Language;
			dateFormatLocale = data.DateFormatLocale;
			firstDayOfWeek = data.FirstDayOfWeek;
			dayNames = data.DayNames;
			dateTimeFormat = data.DateTimeFormat;
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
				DayNames: dayNames || [],
				DateTimeFormat: getDateTimeFormat()
			};
		}

		function getDateTimeFormat() {
			if (!dateTimeFormat) {
				return {};
			}
			var patternArrays = dateTimeFormat.ShortTimePattern.split(' ');
			var showMeridian = patternArrays.length > 1;
			var shortTimePattern = showMeridian ? patternArrays[0] + ' A' : dateTimeFormat.ShortTimePattern;
			return {
				ShortTimePattern: shortTimePattern,
				AMDesignator: dateTimeFormat.AMDesignator,
				PMDesignator: dateTimeFormat.PMDesignator,
				ShowMeridian: showMeridian
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
	}
})();
