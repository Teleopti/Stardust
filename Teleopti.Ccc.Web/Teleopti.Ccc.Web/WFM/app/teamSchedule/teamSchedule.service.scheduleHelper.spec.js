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
				StartInUtc: yesterday + ' 21:00'
			}

		],
		IsFullDayAbsence: false,
		DayOff: null
	};

	it('Should get latest shift start moment of selected schedules', function () {
		scheduleManagementSvc.resetSchedules([schedule1, schedule2], scheduleDate);
		var schedules = scheduleManagementSvc.schedules();
		var timeString = target.getLatestStartMomentOfSelectedSchedules(schedules, scheduleDateMoment, [schedule1.PersonId, schedule2.PersonId])
			.locale('en')
			.format('YYYY-MM-DD HH:mm');
		expect(timeString).toEqual('2016-01-02 12:00');
	});

	it('Should get latest previous day overnight shift end moment', function () {
		scheduleManagementSvc.resetSchedules([schedule1, schedule2, schedule3], scheduleDate);
		var schedules = scheduleManagementSvc.schedules();
		var timeString = target.getLatestPreviousDayOvernightShiftEndMoment(schedules, scheduleDateMoment, [schedule1.PersonId, schedule2.PersonId])
			.locale('en')
			.format('YYYY-MM-DD HH:mm');
		expect(timeString).toEqual('2016-01-02 05:00');
	});

	it('Should get earlist shift start moment of selected schedules', function () {
		scheduleManagementSvc.resetSchedules([schedule1, schedule2, schedule3], scheduleDate);
		var schedules = scheduleManagementSvc.schedules();

		var timeString = target.getEarliestStartMomentOfSelectedSchedules(schedules, scheduleDateMoment, [schedule1.PersonId, schedule2.PersonId])
			.locale('en')
			.format('YYYY-MM-DD HH:mm');
		expect(timeString).toEqual('2016-01-02 11:00');
	});

	it('Should get latest shift start moment of selected projections', function () {
		scheduleManagementSvc.resetSchedules([schedule1, schedule2, schedule3], scheduleDate);
		var schedules = scheduleManagementSvc.schedules();
		schedules.forEach(function (schedule) {
			schedule.Shifts.forEach(function (shift) {
				shift.Projections.forEach(function (projection) {
					projection.Selected = true;
				});
			});
		});
		var timeString = target.getLatestStartTimeMomentOfSelectedProjections(schedules, [schedule1.PersonId, schedule3.PersonId])
			.locale('en')
			.format('YYYY-MM-DD HH:mm');
		expect(timeString).toEqual('2016-01-02 11:00');
	});

});