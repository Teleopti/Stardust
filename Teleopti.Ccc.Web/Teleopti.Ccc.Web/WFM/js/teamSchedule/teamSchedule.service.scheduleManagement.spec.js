"use strict";

describe("teamschedule schedule management service tests", function() {
	var target, fakeTeamScheduleSvc;

	beforeEach(function() {
		module("wfm.teamSchedule");
	});


	beforeEach(function () {
		fakeTeamScheduleSvc = new FakeTeamScheduleSvc();
		module(function ($provide) {
			$provide.service('TeamSchedule', function() {
				return fakeTeamScheduleSvc;
			});
		});
	});

	beforeEach(inject(function (ScheduleManagement) {
		target = ScheduleManagement;
	}));

	function FakeTeamScheduleSvc() {
		var newData = {
			Schedules: [
				{
					"PersonId": "221B-Baker-SomeoneElse",
					"Name": "SomeoneElse",
					"Date": scheduleDate,
					"Projection": [
						{
							"Color": "#80FF80",
							"Description": "Phone",
							"Start": scheduleDate + " 11:00",
							"Minutes": 480
						}
					],
					"IsFullDayAbsence": false,
					"DayOff": null
				},
				{
					"PersonId": "221B-Baker-SomeoneElse",
					"Name": "SomeoneElse",
					"Date": yesterday,
					"Projection": [
						{
							"Color": "#80FF80",
							"Description": "Email",
							"Start": yesterday + " 21:00",
							"Minutes": 480
						}
					],
					"IsFullDayAbsence": false,
					"DayOff": null
				}
			]
		};
		this.getSchedules = function (date, agents) {
			return {
				then: function(cb) {
					cb(newData);
				}
			}
		}
	}

	var scheduleDate = "2016-01-02";
	var yesterday = "2016-01-01";
	var scheduleDateMoment = moment(scheduleDate);
	var schedule1 = {
		"PersonId": "221B-Baker-SomeoneElse",
		"Name": "SomeoneElse",
		"Date": scheduleDate,
		"Projection": [
			{
				"Color": "#80FF80",
				"Description": "Email",
				"Start": scheduleDate + " 11:00",
				"Minutes": 480
			}
		],
		"IsFullDayAbsence": false,
		"DayOff": null
	};
	var schedule2 = {
		"PersonId": "221B-Sherlock",
		"Name": "Sherlock Holmes",
		"Date": scheduleDate,
		"Projection": [
			{
				"Color": "#80FF80",
				"Description": "Email",
				"Start": scheduleDate + " 12:00:00",
				"Minutes": 480
			}
		],
		"IsFullDayAbsence": false,
		"DayOff": null
	};

	var schedule3 = {
		"PersonId": "221B-Baker-SomeoneElse",
		"Name": "SomeoneElse",
		"Date": yesterday,
		"Projection": [
			{
				"Color": "#80FF80",
				"Description": "Email",
				"Start": yesterday + " 21:00",
				"Minutes": 480
			}

		],
		"IsFullDayAbsence": false,
		"DayOff": null
	};

	it("Can create group schedule", inject(function () {
		target.resetSchedules([schedule1, schedule2], scheduleDateMoment);
		var schedules = target.groupScheduleVm.Schedules;
		expect(schedules.length).toEqual(2);
	}));

	it("Should reset group schedule", inject(function () {
		target.resetSchedules([schedule1, schedule2], scheduleDateMoment);
		expect(target.groupScheduleVm.Schedules.length).toEqual(2);

		target.resetSchedules([schedule1], scheduleDateMoment);
		expect(target.groupScheduleVm.Schedules.length).toEqual(1);
	}));

	it("Should update group schedule and keep the order", inject(function () {
		target.resetSchedules([schedule1, schedule3, schedule2], scheduleDateMoment);
		expect(target.groupScheduleVm.Schedules.length).toEqual(2);

		target.updateScheduleForPeoples(["221B-Baker-SomeoneElse"], scheduleDateMoment, function (){});
		expect(target.groupScheduleVm.Schedules.length).toEqual(2);
		expect(target.rawSchedules[0].PersonId).toEqual("221B-Baker-SomeoneElse");
		expect(target.rawSchedules[0].Date).toEqual(scheduleDate);
		expect(target.rawSchedules[0].Projection[0].Description).toEqual("Phone");
	}));

	it("Should get latest shift start in given schedules", function() {
		target.resetSchedules([schedule1, schedule2], scheduleDateMoment);
		expect(moment(target.getLatestStartOfSelectedSchedule(scheduleDateMoment, [schedule1.PersonId,schedule2.PersonId])).format("HH:mm")).toEqual(moment(schedule2.Projection[0].Start).format("HH:mm"));
	});

	it('should get latest previous day overnight shift end', function() {
		target.resetSchedules([schedule1, schedule2, schedule3], scheduleDateMoment);
		expect(target.getLatestPreviousDayOvernightShiftEnd(scheduleDateMoment, [schedule1.PersonId, schedule2.PersonId]).toTimeString()).toEqual(moment(schedule3.Projection[0].Start).add(schedule3.Projection[0].Minutes, 'minute').toDate().toTimeString());
	});

	it('Should get latest shift start independent of timepart of schedule date', function() {
		target.resetSchedules([schedule1, schedule2], scheduleDateMoment);
		scheduleDateMoment.hour(17);
		expect(moment(target.getLatestStartOfSelectedSchedule(scheduleDateMoment, [schedule1.PersonId, schedule2.PersonId])).format("HH:mm")).toEqual(moment(schedule2.Projection[0].Start).format("HH:mm"));
	});

});