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
			return moment.tz(this.nowMoment(), CurrentUserInfo.DefaultTimeZone).format();
		}
		this.getNextTick= function() {
			var nowInUserTimeZoneMoment = moment(this.nowInUserTimeZone());

			var minutes = Math.ceil(nowInUserTimeZoneMoment.minute() / tick) * tick;
			var start = nowInUserTimeZoneMoment.startOf('hour').minutes(minutes);
			return start.format();
		}
	}
})();