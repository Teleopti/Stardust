(function() {
	'use strict';

	angular.module('wfmDate',['currentUserInfoService']).service('WFMDate', WFMDate);

	WFMDate.$inject = ['CurrentUserInfo'];

	function WFMDate(CurrentUserInfo) {
		var nowDate = new Date();
		var tick = 15;
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
		this.getNextTick= function() {
			var minutes = Math.ceil(nowInUserTimeZone.minute() / tick) * tick;
			var start = nowInUserTimeZone.startOf('hour').minutes(minutes);

			return start.format();
		}
	}
})();