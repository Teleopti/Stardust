describe("teamschedule schedule management service tests", function () {
	"use strict";
	var target, fakeTeamScheduleSvc;

	beforeEach(function () {
		module("wfm.teamSchedule");
	});


	var mockCurrentUserInfo = {
		CurrentUserInfo: function () {
			return { DefaultTimeZone: "Asia/Hong_Kong" };
		}
	};

	beforeEach(function () {
		module(function ($provide) {
			$provide.service('CurrentUserInfo', function () {
				return mockCurrentUserInfo;
			});
		});
	});

	beforeEach(function () {
		fakeTeamScheduleSvc = new FakeTeamScheduleSvc();
		module(function ($provide) {
			$provide.service('TeamSchedule', function () {
				return fakeTeamScheduleSvc;
			});
		});
	});

	beforeEach(inject(function (ScheduleManagement) {
		target = ScheduleManagement;
	}));


	function FakeTeamScheduleSvc() {
		var schedules = [];
		this.setSchedules = function (newSchedules) {
			schedules = newSchedules;
		}

		this.getSchedules = function (date, agents) {
			return {
				then: function (cb) {
					cb({
						Schedules: schedules
					});
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
				"StartInUtc": scheduleDate + " 03:00",
				"EndInUtc": scheduleDate + " 11:00"
			}
		],
		"IsFullDayAbsence": false,
		"DayOff": null,
		"UnderlyingScheduleSummary": {
			"PersonalActivities": [{
				"Description": "personal activity",
				"StartInUtc": scheduleDate + ' 02:00',
				"EndInUtc": scheduleDate + ' 03:00'
			}],
			"PersonPartTimeAbsences": [{
				"Description": "holiday",
				"StartInUtc": scheduleDate + ' 03:30',
				"EndInUtc": scheduleDate + ' 04:00'
			}],
			"PersonMeetings": [{
				"Description": "administration",
				"StartInUtc": scheduleDate + ' 06:00',
				"EndInUtc": scheduleDate + ' 07:00'
			}]
		}
	};
	var schedule2 = {
		"PersonId": "221B-Sherlock",
		"Name": "Sherlock Holmes",
		"Date": scheduleDate,
		"Projection": [
			{
				"Color": "#80FF80",
				"Description": "Email",
				"StartInUtc": scheduleDate + " 04:00:00",
				"EndInUtc": scheduleDate + " 12:00:00"
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
				"StartInUtc": yesterday + " 13:00",
				"EndInUtc": yesterday + " 21:00"
			}

		],
		"IsFullDayAbsence": false,
		"DayOff": null
	};

	it("can recreate schedule view model with specified timezone", function () {
		target.resetSchedules([schedule1], scheduleDate, 'Europe/Berlin');

		var schedule = target.groupScheduleVm.Schedules[0];

		expect(schedule.Shifts[0].Projections[0].Start).toEqual(scheduleDate + " 04:00");
		expect(schedule.UnderlyingScheduleSummary.PersonalActivities[0].Start).toEqual(scheduleDate + " 03:00");
		expect(schedule.UnderlyingScheduleSummary.PersonalActivities[0].End).toEqual(scheduleDate + " 04:00");
		expect(schedule.UnderlyingScheduleSummary.PersonPartTimeAbsences[0].Start).toEqual(scheduleDate + " 04:30");
		expect(schedule.UnderlyingScheduleSummary.PersonPartTimeAbsences[0].End).toEqual(scheduleDate + " 05:00");
		expect(schedule.UnderlyingScheduleSummary.PersonMeetings[0].Start).toEqual(scheduleDate + " 07:00");
		expect(schedule.UnderlyingScheduleSummary.PersonMeetings[0].End).toEqual(scheduleDate + " 08:00");
	});

	it("should reset group schedule", inject(function () {
		target.resetSchedules([schedule1, schedule2], scheduleDate);
		expect(target.groupScheduleVm.Schedules.length).toEqual(2);

		target.resetSchedules([schedule1], scheduleDate);
		expect(target.groupScheduleVm.Schedules.length).toEqual(1);
	}));

	it("should update group schedule for people and keep the order", inject(function () {
		target.resetSchedules([schedule1, schedule3, schedule2], scheduleDate);

		fakeTeamScheduleSvc.setSchedules([
			{
				"PersonId": "221B-Baker-SomeoneElse",
				"Name": "SomeoneElse",
				"Date": scheduleDate,
				"Projection": [
					{
						"Color": "#80FF80",
						"Description": "Phone",
						"StartInUtc": scheduleDate + " 03:00",
						"EndInUtc": scheduleDate + " 11:00"
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
						"StartInUtc": yesterday + " 13:00",
						"EndInUtc": yesterday + " 21:00"
					}
				],
				"IsFullDayAbsence": false,
				"DayOff": null
			}
		]);

		target.updateScheduleForPeoples(["221B-Baker-SomeoneElse"], scheduleDate);


		expect(target.groupScheduleVm.Schedules.length).toEqual(2);
		expect(target.rawSchedules[0].PersonId).toEqual("221B-Baker-SomeoneElse");
		expect(target.rawSchedules[0].Date).toEqual(scheduleDate);
		expect(target.rawSchedules[0].Projection[0].Description).toEqual("Phone");
	}));

	it('should find person schedule vm for person', function () {
		target.resetSchedules([schedule1, schedule3, schedule2], scheduleDate);
		var scheduleVm = target.findPersonScheduleVmForPersonId('221B-Baker-SomeoneElse');
		expect(scheduleVm.PersonId).toEqual('221B-Baker-SomeoneElse');

		expect(scheduleVm.Shifts[0].Projections[0].Start).toEqual(scheduleDate + " 11:00");
	});


	it('should adjust start position when time line length is changed by updating schedules', function () {
		var scheduleForPerson1 = {
			"PersonId": "person1",
			"Name": "person1",
			"Date": '2019-01-10',
			"Projection": [
				{
					"ShiftLayerIds": ["31ffe214-3384-4a80-a14c-a83800e23276"],
					"Color": "#795548",
					"Description": "Phone",
					"StartInUtc": "2019-01-10 08:00",
					"EndInUtc": "2019-01-10 16:00",
					"IsOvertime": false
				}
			],
			"DayOff": null
		};
		var scheduleForPerson2 = {
			"PersonId": "person2",
			"Name": "person2",
			"Date": '2019-01-10',
			"Projection": [
				{
					"ShiftLayerIds": ["31ffe214-3384-4a80-a14c-a83800e23276"],
					"Color": "#795548",
					"Description": "Phone",
					"StartInUtc": "2019-01-10 08:00",
					"EndInUtc": "2019-01-10 16:00",
					"IsOvertime": false
				}
			],
			"DayOff": null
		};

		target.resetSchedules([scheduleForPerson1, scheduleForPerson2], '2019-01-10');


		fakeTeamScheduleSvc.setSchedules([
			{
				"PersonId": "person2",
				"Name": "person2",
				"Date": '2019-01-10',
				"Projection": [
					{
						"ShiftLayerIds": ["31ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#795548",
						"Description": "Phone",
						"StartInUtc": "2019-01-10 22:00",
						"EndInUtc": "2019-01-11 07:00",
						"IsOvertime": false
					}
				],
				"IsFullDayAbsence": false,
				"DayOff": null
			}
		]);

		target.updateScheduleForPeoples(["person2"], "2019-01-10");
		expect(target.groupScheduleVm.Schedules[0].Shifts[0].Projections[0].StartPosition)
			.toEqual(target.groupScheduleVm.TimeLine.LengthPercentPerMinute * 60);
	});

});