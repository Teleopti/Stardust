(function() {
	'use strict';

	angular.module('wfmDate',['currentUserInfoService']).service('WFMDate', WFMDate);

	WFMDate.$inject = ['CurrentUserInfo'];

	function WFMDate(CurrentUserInfo) {
		var nowDate = new Date();
		this.setNowDate = function(date) {
			nowDate = date;
		}
		this.now = function() {
			return nowDate;
		};
		this.nowMoment = function() {
			return moment(nowDate);
		}
		this.nowInUserTimeZone = function () {
			return moment(this.nowMoment(), CurrentUserInfo.DefaultTimeZone);
		}
	}

})();