'use strict';

angular.module('currentUserInfoService', []).service('CurrentUserInfo', [function() {
		var userName;
		var defaultTimeZone;
		var language;
		var dateFormatLocale;
		var numberFormat;

		this.SetCurrentUserInfo = function(data) {
			userName = data.UserName;
			defaultTimeZone = data.DefaultTimeZone;
			language = data.Language;
			dateFormatLocale = data.DateFormatLocale;
			numberFormat = data.NumberFormat;
		}

		this.CurrentUserInfo = function() {
			return {
				UserName: userName,
				DefaultTimeZone: defaultTimeZone,
				Language: language,
				DateFormatLocale: dateFormatLocale,
				NumberFormat: numberFormat
			}
		};
	}
]);
