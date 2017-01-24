describe('teamschedule ScheduleHelper Service tests', function () {
	'use strict';

    var target;
    var scheduleManagementSvc;

    beforeEach(function() {
		module('wfm.teamSchedule');
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
				Start: scheduleDate + ' 11:00',
				Minutes: 480
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
				Start: scheduleDate + ' 12:00:00',
				Minutes: 480
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
				Start: yesterday + ' 21:00',
				Minutes: 480
			}

		],
		IsFullDayAbsence: false,
		DayOff: null
	};

    it('Should get latest shift start in given schedules', function () {
        var schedules;

        scheduleManagementSvc.resetSchedules([schedule1, schedule2], scheduleDateMoment);
		schedules = scheduleManagementSvc.schedules();

        expect(moment(target.getLatestStartOfSelectedSchedules(schedules, scheduleDateMoment, [schedule1.PersonId, schedule2.PersonId])).format('HH:mm')).toEqual(moment(schedule2.Projection[0].Start).format('HH:mm'));
    });

    it('Should get latest previous day overnight shift end', function () {
        var schedules;

        scheduleManagementSvc.resetSchedules([schedule1, schedule2, schedule3], scheduleDateMoment);
		schedules = scheduleManagementSvc.schedules();

        expect(target.getLatestPreviousDayOvernightShiftEnd(schedules, scheduleDateMoment, [schedule1.PersonId, schedule2.PersonId]).toTimeString()).toEqual(moment(schedule3.Projection[0].Start).add(schedule3.Projection[0].Minutes, 'minute').toDate().toTimeString());
    });

    it('Should get latest shift start independent of timepart of schedule date', function () {
        var schedules;

        scheduleManagementSvc.resetSchedules([schedule1, schedule2], scheduleDateMoment);
		schedules = scheduleManagementSvc.schedules();

        scheduleDateMoment.hour(17);
        expect(moment(target.getLatestStartOfSelectedSchedules(schedules, scheduleDateMoment, [schedule1.PersonId, schedule2.PersonId])).format('HH:mm')).toEqual(moment(schedule2.Projection[0].Start).format('HH:mm'));
    });

});