'use strict';

angular.module('currentUserInfoService', []).service('CurrentUserInfo', ['$http', '$q', function ($http, $q) {
		var userName;
		var defaultTimeZone;
		var language;
		var dateFormatLocale;
		var numberFormat;
		var timeout;
		
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
		
		this.getCurrentUserFromServer = function() {
			return $http.get('../api/Global/User/CurrentUser');
		}

		this.initContext = function () {
			var deferred = $q.defer();
			var context = this.getCurrentUserFromServer();

			context.success(function (data) {
				timeout = Date.now() + 10000;
				deferred.resolve(data);
			});
			return deferred.promise; 
		};

		this.isConnected = function() {
			return timeout > Date.now();
		}
	}
]);
