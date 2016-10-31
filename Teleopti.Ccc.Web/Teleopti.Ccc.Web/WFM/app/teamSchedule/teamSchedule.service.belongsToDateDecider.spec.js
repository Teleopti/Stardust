﻿describe ('belongs to date decider service tests', function() {

	var target;

	beforeEach(module('wfm.teamSchedule'));

	beforeEach(inject(function(_belongsToDateDecider_) {
		target = _belongsToDateDecider_;
	}));

	it('should decide the correct date if the target range intersects with the existing shift', function() {
		var normalizedScheduleDataArray = [
			{
				date: '2016-07-01',
				timeRange: {
					startTime: moment('2016-07-01 00:00'),
					endTime: moment('2016-07-02 00:00')
				},
				shiftRange: {
					startTime: moment('2016-07-01 23:00'),
					endTime: moment('2016-07-02 03:00')
				}
			},
			{
				date: '2016-07-02',
				timeRange: {
					startTime: moment('2016-07-02 00:00'),
					endTime: moment('2016-07-03 00:00')
				},
				shiftRange: {
					startTime: moment('2016-07-02 08:00'),
					endTime: moment('2016-07-02 16:00')
				}
			},
			{
				date: '2016-07-03',
				timeRange: {
					startTime: moment('2016-07-03 00:00'),
					endTime: moment('2016-07-04 00:00')
				},
				shiftRange: {
					startTime: moment('2016-07-03 08:00'),
					endTime: moment('2016-07-03 16:00')
				}
			}
		];

		var targetTimeRange = { startTime: moment('2016-07-02 04:00'), endTime: moment('2016-07-02 08:00') };

		var result = target.decideBelongsToDate(targetTimeRange, normalizedScheduleDataArray, '2016-07-02');

		expect(result).toEqual('2016-07-02');

	});

	it('should decide the correct date if the target range start from day without shift', function () {
		var normalizedScheduleDataArray = [
			{
				date: '2016-07-01',
				timeRange: {
					startTime: moment('2016-07-01 00:00'),
					endTime: moment('2016-07-02 00:00')
				},
				shiftRange: {
					startTime: moment('2016-07-01 08:00'),
					endTime: moment('2016-07-01 16:00')
				}
			},
			{
				date: '2016-07-02',
				timeRange: {
					startTime: moment('2016-07-02 00:00'),
					endTime: moment('2016-07-03 00:00')
				},
				shiftRange: null
			},
			{
				date: '2016-07-03',
				timeRange: {
					startTime: moment('2016-07-03 00:00'),
					endTime: moment('2016-07-04 00:00')
				},
				shiftRange: {
					startTime: moment('2016-07-03 08:00'),
					endTime: moment('2016-07-03 16:00')
				}
			}
		];

		var targetTimeRange = { startTime: moment('2016-07-02 22:00'), endTime: moment('2016-07-03 04:00') };

		var result = target.decideBelongsToDate(targetTimeRange, normalizedScheduleDataArray, '2016-07-02');

		expect(result).toEqual('2016-07-02');

	});

	it('should fail to decide if target range intersects with shifts in different days', function () {
		var normalizedScheduleDataArray = [
			{
				date: '2016-07-01',
				timeRange: {
					startTime: moment('2016-07-01 00:00'),
					endTime: moment('2016-07-02 00:00')
				},
				shiftRange: {
					startTime: moment('2016-07-01 23:00'),
					endTime: moment('2016-07-02 06:00')
				}
			},
			{
				date: '2016-07-02',
				timeRange: {
					startTime: moment('2016-07-02 00:00'),
					endTime: moment('2016-07-03 00:00')
				},
				shiftRange: {
					startTime: moment('2016-07-02 08:00'),
					endTime: moment('2016-07-02 16:00')
				}
			},
			{
				date: '2016-07-03',
				timeRange: {
					startTime: moment('2016-07-03 00:00'),
					endTime: moment('2016-07-04 00:00')
				},
				shiftRange: {
					startTime: moment('2016-07-03 08:00'),
					endTime: moment('2016-07-03 16:00')
				}
			}
		];

		var targetTimeRange = { startTime: moment('2016-07-02 04:00'), endTime: moment('2016-07-02 09:00') };

		var result = target.decideBelongsToDate(targetTimeRange, normalizedScheduleDataArray, '2016-07-02');

		expect(result).toBeNull();

	});

	it('should fail to decide if target range start from a day without shift but does not intersect it', function () {
		var normalizedScheduleDataArray = [
			{
				date: '2016-07-01',
				timeRange: {
					startTime: moment('2016-07-01 00:00'),
					endTime: moment('2016-07-02 00:00')
				},
				shiftRange: {
					startTime: moment('2016-07-01 23:00'),
					endTime: moment('2016-07-02 03:00')
				}
			},
			{
				date: '2016-07-02',
				timeRange: {
					startTime: moment('2016-07-02 00:00'),
					endTime: moment('2016-07-03 00:00')
				},
				shiftRange: {
					startTime: moment('2016-07-02 08:00'),
					endTime: moment('2016-07-02 16:00')
				}
			},
			{
				date: '2016-07-03',
				timeRange: {
					startTime: moment('2016-07-03 00:00'),
					endTime: moment('2016-07-04 00:00')
				},
				shiftRange: {
					startTime: moment('2016-07-03 08:00'),
					endTime: moment('2016-07-03 16:00')
				}
			}
		];

		var targetTimeRange = { startTime: moment('2016-07-02 04:00'), endTime: moment('2016-07-02 05:00') };

		var result = target.decideBelongsToDate(targetTimeRange, normalizedScheduleDataArray, '2016-07-02');

		expect(result).toBeNull();

	});

	it('should fail to decide if the target range merged with the existing shift becomes too long', function () {
		var normalizedScheduleDataArray = [
			{
				date: '2016-07-01',
				timeRange: {
					startTime: moment('2016-07-01 00:00'),
					endTime: moment('2016-07-02 00:00')
				},
				shiftRange: null
			},
			{
				date: '2016-07-02',
				timeRange: {
					startTime: moment('2016-07-02 00:00'),
					endTime: moment('2016-07-03 00:00')
				},
				shiftRange: {
					startTime: moment('2016-07-02 01:00'),
					endTime: moment('2016-07-02 23:00')
				}
			},
			{
				date: '2016-07-03',
				timeRange: {
					startTime: moment('2016-07-03 00:00'),
					endTime: moment('2016-07-04 00:00')
				},
				shiftRange: null
			}
		];

		var targetTimeRange = { startTime: moment('2016-07-02 22:00'), endTime: moment('2016-07-03 14:00') };

		var result = target.decideBelongsToDate(targetTimeRange, normalizedScheduleDataArray, '2016-07-02');

		expect(result).toBeNull();

	});

	it('should fail to decide if the target range merged with the existing shift starts before the belongs-to-date', function () {
		var normalizedScheduleDataArray = [
			{
				date: '2016-07-01',
				timeRange: {
					startTime: moment('2016-07-01 10:00'),
					endTime: moment('2016-07-02 10:00')
				},
				shiftRange: null
			},
			{
				date: '2016-07-02',
				timeRange: {
					startTime: moment('2016-07-02 10:00'),
					endTime: moment('2016-07-03 10:00')
				},
				shiftRange: {
					startTime: moment('2016-07-02 13:00'),
					endTime: moment('2016-07-03 00:00')
				}
			},
			{
				date: '2016-07-03',
				timeRange: {
					startTime: moment('2016-07-03 10:00'),
					endTime: moment('2016-07-04 10:00')
				},
				shiftRange: null
			}
		];

		var targetTimeRange = { startTime: moment('2016-07-02 08:00'), endTime: moment('2016-07-02 14:00') };

		var result = target.decideBelongsToDate(targetTimeRange, normalizedScheduleDataArray, '2016-07-02');

		expect(result).toBeNull();

	});

	it('should get normalized schedule data', function () {
		var timezone1 = {
			IanaId: "Asia/Shanghai",
			DisplayName: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"
		};
		var currentTimezone = "Europe/Berlin";

		var personScheduleVm = {
			Date: '2016-07-02',
			Timezone: timezone1,
			Shifts: [
			{
				Date: '2016-07-01',
				Projections: [],
				ProjectionTimeRange: null
			},
			{
				Date: '2016-07-02',
				Projections: [
				{
					Start: '2016-07-02 08:00',
					End: '2016-07-02 17:00',
					Minutes: 540
				}],
				ProjectionTimeRange: {
					Start: '2016-07-02 08:00',
					End: '2016-07-02 17:00'
				}
			}]
		};

		var result = target.normalizePersonScheduleVm(personScheduleVm, currentTimezone);

		expect(result.length).toEqual(3);
		expect(result[0].date).toEqual('2016-07-01');
		expect(result[0].timeRange.startTime.format('YYYY-MM-DD HH:mm')).toEqual('2016-06-30 18:00');
		expect(result[0].timeRange.endTime.format('YYYY-MM-DD HH:mm')).toEqual('2016-07-01 18:00');
		expect(result[0].shiftRange).toBeNull();
		expect(result[1].date).toEqual('2016-07-02');
		expect(result[1].timeRange.startTime.format('YYYY-MM-DD HH:mm')).toEqual('2016-07-01 18:00');
		expect(result[1].timeRange.endTime.format('YYYY-MM-DD HH:mm')).toEqual('2016-07-02 18:00');
		expect(result[1].shiftRange.startTime.format('YYYY-MM-DD HH:mm')).toEqual('2016-07-02 08:00');
		expect(result[1].shiftRange.endTime.format('YYYY-MM-DD HH:mm')).toEqual('2016-07-02 17:00');
		expect(result[2].date).toEqual('2016-07-03');
		expect(result[2].timeRange.startTime.format('YYYY-MM-DD HH:mm')).toEqual('2016-07-02 18:00');
		expect(result[2].timeRange.endTime.format('YYYY-MM-DD HH:mm')).toEqual('2016-07-03 18:00');
		expect(result[2].shiftRange).toBeNull();
	});

});