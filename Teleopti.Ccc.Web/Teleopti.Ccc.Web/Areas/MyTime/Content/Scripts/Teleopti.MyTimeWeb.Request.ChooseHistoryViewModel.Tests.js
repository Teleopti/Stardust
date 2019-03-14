$(document).ready(function() {
	module('Teleopti.MyTimeWeb.Request.ShiftTradeRequestDetailViewModel');

	test('Should use meridian correctly when calculating timeline', function() {
		var mySchedule = {
				isDayOff: false,
				scheduleStartTime: function() {
					return moment('2015-01-01 21:00:00');
				},
				scheduleEndTime: function() {
					return moment('2015-01-01 23:00:00');
				}
			},
			tradedSchedule = {
				isDayOff: false,
				scheduleStartTime: function() {
					return moment('2015-01-01 7:00:00');
				},
				scheduleEndTime: function() {
					return moment('2015-01-01 19:00:00');
				}
			};

		var result = Teleopti.MyTimeWeb.Request.ChooseHistoryViewModelUtil.findScheduleTimeRange(
			mySchedule,
			tradedSchedule
		);

		equal(result.scheduleStartTime.format('HH:mm'), '07:00');
		equal(result.scheduleEndTime.format('HH:mm'), '23:00');
	});
});
