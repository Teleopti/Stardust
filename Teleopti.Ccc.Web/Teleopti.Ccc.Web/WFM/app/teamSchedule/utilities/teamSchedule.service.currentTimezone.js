(function () {
	'use strict';

	angular.module('wfm.teamSchedule').service('currentTimezone', currentTimezone);

	currentTimezone.$inject = ['CurrentUserInfo'];

	function currentTimezone(currentUserInfo) {
		var self = this;

		var currentTimezone = currentUserInfo.CurrentUserInfo().DefaultTimeZone;

		self.set = function (data) {
			currentTimezone = data;
		}

		self.get = function () {
			return currentTimezone;
		}
	}

})();