describe('teamschedule ScheduleHelper Service tests', function () {
	'use strict';

	var target;
	var scheduleManagementSvc;
	var mockCurrentUserInfo = {
		CurrentUserInfo: function () {
			return { DefaultTimeZone: "etc/UTC" };
		}
	};

	beforeEach(function () {
		module('wfm.teamSchedule');
	});

	beforeEach(function () {
		module(function ($provide) {
			$provide.service('CurrentUserInfo', function () {
				return mockCurrentUserInfo;
			});
		});
	});

	beforeEach(inject(function (ScheduleHelper, ScheduleManagement) {
		target = ScheduleHelper;
		scheduleManagementSvc = ScheduleManagement;
	}));

	var scheduleDate = '2016-01-02';
	var yesterday = '2016-01-01';
	var scheduleDateMoment = moment(scheduleDate);

	var schedule1 = {
		PersonId: '221B-Baker-SomeoneElse',
		Name: 'SomeoneElse',
		Date: scheduleDate,
		Projection: [
			{
				Color: '#80FF80',
				Description: 'Email',
				StartInUtc: scheduleDate + ' 11:00',
				EndInUtc: scheduleDate + ' 19:00'
			}
		],
		IsFullDayAbsence: false,
		DayOff: null
	};

	var schedule2 = {
		PersonId: '221B-Sherlock',
		Name: 'Sherlock Holmes',
		Date: scheduleDate,
		Projection: [
			{
				Color: '#80FF80',
				Description: 'Email',
				EndInUtc: scheduleDate + ' 20:00:00',
				StartInUtc: scheduleDate + ' 12:00:00'
			}
		],
		IsFullDayAbsence: false,
		DayOff: null
	};

	var schedule3 = {
		PersonId: '221B-Baker-SomeoneElse',
		Name: 'SomeoneElse',
		Date: yesterday,
		Projection: [
			{
				Color: '#80FF80',
				Description: 'Email',
				EndInUtc: scheduleDate + ' 05:00',
				StartInUtc: yesterday + ' 21:00',
				Minutes: 480
			}

		],
		IsFullDayAbsence: false,
		DayOff: null
	};

	it('Should get latest shift start in given schedules', function () {
		var schedules;

		scheduleManagementSvc.resetSchedules([schedule1, schedule2], scheduleDate);
		schedules = scheduleManagementSvc.schedules();

		expect(moment(target.getLatestStartOfSelectedSchedules(schedules, scheduleDateMoment, [schedule1.PersonId, schedule2.PersonId])).format('HH:mm')).toEqual(moment(schedule2.Projection[0].StartInUtc).format('HH:mm'));
	});

	it('Should get latest previous day overnight shift end', function () {
		var schedules;

		scheduleManagementSvc.resetSchedules([schedule1, schedule2, schedule3], scheduleDate);
		schedules = scheduleManagementSvc.schedules();

		expect(target.getLatestPreviousDayOvernightShiftEnd(schedules, scheduleDateMoment, [schedule1.PersonId, schedule2.PersonId]).toTimeString()).toEqual(moment(schedule3.Projection[0].StartInUtc).add(schedule3.Projection[0].Minutes, 'minute').toDate().toTimeString());
	});

	it('Should get latest shift start independent of timepart of schedule date', function () {
		var schedules;

		scheduleManagementSvc.resetSchedules([schedule1, schedule2], scheduleDate);
		schedules = scheduleManagementSvc.schedules();

		scheduleDateMoment.hour(17);
		expect(moment(target.getLatestStartOfSelectedSchedules(schedules, scheduleDateMoment, [schedule1.PersonId, schedule2.PersonId])).format('HH:mm')).toEqual(moment(schedule2.Projection[0].StartInUtc).format('HH:mm'));
	});

});