﻿"use strict";

describe("test of PersonSchedule", function () {
	var dateTimeFormat = "YYYY-MM-DD HH:mm:ss";
	var target;

	beforeEach(function() {
		module("wfm.teamSchedule");
		module(function($provide) {
			$provide.value("CurrentUserInfoStub", {
				DefaultTimeZone: "Etc/UTC"
			});
		});
	});


	beforeEach(inject(function (PersonSchedule) {
		target = PersonSchedule;
	}));
	
	it("Can get correct projection", inject(function () {
		var queryDate = "2015-10-26";

		var timeLineStart = 480;
		var timeLineEnd = 1200;

		var timeLine = {
			Offset: moment(queryDate),
			StartMinute: timeLineStart,
			EndMinute: timeLineEnd,
			LengthPercentPerMinute: 100 / (timeLineEnd - timeLineStart)
		};

		var schedule = {
			"PersonId": "221B-Baker-Street",
			"Name": "Sherlock Holmes",
			"Date": queryDate,
			"Projection": [
				{
					"Color": "Red",
					"Description": "Phone",
					"Start": queryDate + " 08:00",
					"Minutes": 120
				},
				{
					"Color": "Yellow",
					"Description": "Email",
					"Start": queryDate + " 10:00",
					"Minutes": 360 // End = "2015-10-26 16:00"
				}
			],
			DayOff: null
		};

		var personSchedule = target.Create(schedule, timeLine);

		expect(personSchedule.PersonId).toEqual(schedule.PersonId);
		expect(personSchedule.Name).toEqual(schedule.Name);
		expect(personSchedule.Date.format(dateTimeFormat)).toEqual(moment(schedule.Date).format(dateTimeFormat));

		expect(personSchedule.Shifts.length).toEqual(1);
		expect(personSchedule.DayOffs.length).toEqual(0);

		verifyShift(timeLine, personSchedule.Shifts[0], schedule);
	}));

	it("Can get correct dayoff schedule", inject(function () {
		var queryDate = "2015-10-26";

		var timeLineStart = 480;
		var timeLineEnd = 1200;

		var timeLine = {
			Offset: moment(queryDate),
			StartMinute: timeLineStart,
			EndMinute: timeLineEnd,
			LengthPercentPerMinute: 100 / (timeLineEnd - timeLineStart)
		};

		var schedule = {
			"PersonId": "221B-Baker-Street",
			"Name": "Sherlock Holmes",
			"Date": queryDate,
			"Projection": [],
			"DayOff":
			{
				"DayOffName": "Day off",
				"Start": "2015-10-26 00:00",
				"Minutes": 1440
			}
		};

		var personSchedule = target.Create(schedule, timeLine);

		expect(personSchedule.PersonId).toEqual(schedule.PersonId);
		expect(personSchedule.Name).toEqual(schedule.Name);
		expect(personSchedule.Date.format(dateTimeFormat)).toEqual(moment(schedule.Date).format(dateTimeFormat));
		expect(personSchedule.Shifts.length).toEqual(0);
		expect(personSchedule.DayOffs.length).toEqual(1);

		verifyDayOff(timeLine, personSchedule.DayOffs[0], schedule.DayOff);
	}));

	it("Should merge schedules for same people", inject(function () {
		var queryDate = "2015-10-26";
		var yesterday = moment(queryDate).add(-1, "days").startOf("days").format("YYYY-MM-DD");

		var timeLineStart = 0;
		var timeLineEnd = 1440;

		var timeLine = {
			Offset: moment(queryDate),
			StartMinute: timeLineStart,
			EndMinute: timeLineEnd,
			LengthPercentPerMinute: 100 / (timeLineEnd - timeLineStart)
		};

		var schedule = {
			"PersonId": "221B-Baker-Street",
			"Name": "Sherlock Holmes",
			"Date": yesterday,
			"Projection": [
				{
					"Color": "Red",
					"Description": "Phone",
					"Start": yesterday + " 20:00",
					"Minutes": 480 // End = "2015-10-26 04:00"
				}
			],
			DayOff: null
		};

		var anotherSchedule = {
			"PersonId": "221B-Baker-Street",
			"Name": "Sherlock Holmes",
			"Date": queryDate,
			"Projection": [
				{
					"Color": "Red",
					"Description": "Phone",
					"Start": queryDate + " 15:00",
					"Minutes": 120
				},
				{
					"Color": "Yellow",
					"Description": "Email",
					"Start": queryDate + " 17:00",
					"Minutes": 360 // End = "2015-10-26 23:00"
				}
			],
			DayOff: null
		};

		var personSchedule = target.Create(schedule, timeLine);

		personSchedule.Merge(anotherSchedule, timeLine);

		expect(personSchedule.PersonId).toEqual(schedule.PersonId);
		expect(personSchedule.Name).toEqual(schedule.Name);
		expect(personSchedule.Date.format(dateTimeFormat)).toEqual(moment(yesterday).format(dateTimeFormat));
		expect(personSchedule.Shifts.length).toEqual(2);
		expect(personSchedule.DayOffs.length).toEqual(0);

		verifyShift(timeLine, personSchedule.Shifts[0], schedule);
		verifyShift(timeLine, personSchedule.Shifts[1], anotherSchedule);
	}));

	it("Should merge dayoff for same people", inject(function () {
		var queryDate = "2015-10-30";
		var yesterday = moment(queryDate).add(-1, "days").startOf("days").format("YYYY-MM-DD");

		var timeLineStart = 1200; // 2015-10-26 20:00
		var timeLineEnd = 2640;   // 2015-10-27 20:00

		var timeLine = {
			Offset: moment(yesterday),
			StartMinute: timeLineStart,
			EndMinute: timeLineEnd,
			LengthPercentPerMinute: 100 / (timeLineEnd - timeLineStart)
		};

		var schedule4Yesterday = {
			"PersonId": "221B-Baker-Street",
			"Name": "Sherlock Holmes",
			"Date": yesterday,
			"Projection": [],
			"DayOff":
			{
				"DayOffName": "Day off",
				"Start": yesterday + " 00:00",
				"Minutes": 1440
			}
		};

		var schedule4Today = {
			"PersonId": "221B-Baker-Street",
			"Name": "Sherlock Holmes",
			"Date": queryDate,
			"Projection": [],
			"DayOff":
			{
				"DayOffName": "Shoft DayOff",
				"Start": queryDate + " 00:00",
				"Minutes": 1440
			}
		};

		var personSchedule = target.Create(schedule4Yesterday, timeLine);
		personSchedule.Merge(schedule4Today, timeLine);

		expect(personSchedule.PersonId).toEqual(schedule4Yesterday.PersonId);
		expect(personSchedule.Name).toEqual(schedule4Yesterday.Name);
		expect(personSchedule.Date.format(dateTimeFormat)).toEqual(moment(schedule4Yesterday.Date).format(dateTimeFormat));
		expect(personSchedule.Shifts.length).toEqual(0);
		expect(personSchedule.DayOffs.length).toEqual(2);

		verifyDayOff(timeLine, personSchedule.DayOffs[0], schedule4Yesterday.DayOff);
		verifyDayOff(timeLine, personSchedule.DayOffs[1], schedule4Today.DayOff);
	}));

	it("should get correct schedule start time", function() {
		var queryDate = "2015-10-30";
		var timeLineStart = 0;
		var timeLineEnd = 1440;

		var timeLine = {
			Offset: moment(queryDate),
			StartMinute: timeLineStart,
			EndMinute: timeLineEnd,
			LengthPercentPerMinute: 100 / (timeLineEnd - timeLineStart)
		};
		var scheduleYesterday = {
			"PersonId": "221B-Baker-Street",
			"Name": "Sherlock Holmes",
			"Date": new Date('2015-10-29T00:00:00Z'),
			"Projection": [],
			"DayOff":
			{
				"DayOffName": "Day off",
				"Start": "2015-10-30T07:00:00",
				"Minutes": 1440
			}
		};

		var scheduleToday = {
			"PersonId": "221B-Baker-Street",
			"Name": "Sherlock Holmes",
			"Date": new Date('2015-10-30T00:00:00Z'),
			"Projection": [
			{
				"Color": "#80FF80",
				"Description": "Email",
				"Start": '2015-10-30T07:00:00',
				"Minutes": 480
			}],
			"DayOff":null
		};
		var personSchedule = target.Create(scheduleToday, timeLine);
		personSchedule.Merge(scheduleYesterday, timeLine);

		expect(personSchedule.ScheduleStartTime()).toEqual('2015-10-30T07:00:00');
	});

	it("should get correct schedule end time", function() {
		var queryDate = "2015-10-30T00:00:00";
		var timeLineStart = 0;
		var timeLineEnd = 1440;

		var timeLine = {
			Offset: moment(queryDate),
			StartMinute: timeLineStart,
			EndMinute: timeLineEnd,
			LengthPercentPerMinute: 100 / (timeLineEnd - timeLineStart)
		};
		var scheduleYesterday = {
			"PersonId": "221B-Baker-Street",
			"Name": "Sherlock Holmes",
			"Date": new Date('2015-10-29T00:00:00'),
			"Projection": [],
			"DayOff":
			{
				"DayOffName": "Day off",
				"Start": '2015-10-29T08:00:00',
				"Minutes": 1440
			}
		};

		var scheduleToday = {
			"PersonId": "221B-Baker-Street",
			"Name": "Sherlock Holmes",
			"Date": queryDate,
			"Projection": [
			{
				"Color": "#80FF80",
				"Description": "Email",
				"Start": "2015-10-30T07:00:00",
				"Minutes": 480
			}],
			"DayOff":null
		};
		var personSchedule = target.Create(scheduleToday, timeLine);
		personSchedule.Merge(scheduleYesterday, timeLine);

		expect(personSchedule.ScheduleEndTime()).toEqual(moment(queryDate).endOf('day').format("YYYY-MM-DDTHH:mm:00"));
	});

	it('Should get correct person activities count', function() {
		var queryDate = "2015-10-30";
		var yesterday = moment(queryDate).add(-1, "days").startOf("days").format("YYYY-MM-DD");
		var timeLineStart = 0;
		var timeLineEnd = 1440;

		var timeLine = {
			Offset: moment(queryDate),
			StartMinute: timeLineStart,
			EndMinute: timeLineEnd,
			LengthPercentPerMinute: 100 / (timeLineEnd - timeLineStart)
		};
		var scheduleYesterday = {
			"PersonId": "221B-Baker-Street",
			"Name": "Sherlock Holmes",
			"Date": yesterday,
			"Projection": [
			{
			    "ShiftLayerIds": ["111"],
				"Color": "#80FF80",
				"Description": "Email",
				"Start": yesterday + " 19:00",
				"Minutes": 480
			}],
			"DayOff":null
		};

		var scheduleToday = {
			"PersonId": "221B-Baker-Street",
			"Name": "Sherlock Holmes",
			"Date": queryDate,
			"Projection": [
			{
			    "ShiftLayerIds": ["222"],
				"Color": "#80FF80",
				"Description": "Email",
				"Start": queryDate + " 07:00",
				"Minutes": 240
			},
			{
			    "ShiftLayerIds": ["333"],
				"Color": "#80FF80",
				"Description": "Email",
				"Start": queryDate + " 11:00",
				"Minutes": 240
			}],
			"DayOff": null
		};
		var personSchedule = target.Create(scheduleToday, timeLine);
		personSchedule.Merge(scheduleYesterday, timeLine);
		expect(personSchedule.Shifts[0].ActivityCount()).toEqual(2);
		expect(personSchedule.Shifts[1].ActivityCount()).toEqual(1);
	});

	it('Should get correct formatted contact time', function() {
		var queryDate = "2015-10-30";
		var yesterday = moment(queryDate).add(-1, "days").startOf("days").format("YYYY-MM-DD");
		var timeLineStart = 0;
		var timeLineEnd = 1440;

		var timeLine = {
			Offset: moment(queryDate),
			StartMinute: timeLineStart,
			EndMinute: timeLineEnd,
			LengthPercentPerMinute: 100 / (timeLineEnd - timeLineStart)
		};
		var scheduleToday = {
			"PersonId": "221B-Baker-Street",
			"Name": "Sherlock Holmes",
			"Date": queryDate,
			"ContractTimeMinutes": 480,
			"Projection": [
			{
			    "ShiftLayerIds": ["222"],
				"Color": "#80FF80",
				"Description": "Email",
				"Start": queryDate + " 07:00",
				"Minutes": 240
			},
			{
			    "ShiftLayerIds": ["333"],
				"Color": "#80FF80",
				"Description": "Email",
				"Start": queryDate + " 11:00",
				"Minutes": 1230
			}],
			"DayOff": null
		};
		var personSchedule = target.Create(scheduleToday, timeLine);
		expect(personSchedule.ContractTime).toEqual("8:00");
	});

	it('Should get correct formatted contact time when it is greater than 24 hours', function() {
		var queryDate = "2015-10-30";
		var yesterday = moment(queryDate).add(-1, "days").startOf("days").format("YYYY-MM-DD");
		var timeLineStart = 0;
		var timeLineEnd = 1440;

		var timeLine = {
			Offset: moment(queryDate),
			StartMinute: timeLineStart,
			EndMinute: timeLineEnd,
			LengthPercentPerMinute: 100 / (timeLineEnd - timeLineStart)
		};
		var scheduleToday = {
			"PersonId": "221B-Baker-Street",
			"Name": "Sherlock Holmes",
			"Date": queryDate,
			"ContractTimeMinutes": 1470,
			"Projection": [
			{
			    "ShiftLayerIds": ["222"],
				"Color": "#80FF80",
				"Description": "Email",
				"Start": queryDate + " 07:00",
				"Minutes": 240
			},
			{
			    "ShiftLayerIds": ["333"],
				"Color": "#80FF80",
				"Description": "Email",
				"Start": queryDate + " 11:00",
				"Minutes": 240
			}],
			"DayOff": null
		};
		var personSchedule = target.Create(scheduleToday, timeLine);
		expect(personSchedule.ContractTime).toEqual("24:30");
	});

	function verifyShift(timeLine, shift, rawSchedule) {
		shift.Projections.forEach(function (projection, index) {
			var rawProjection = rawSchedule.Projection[index];
			expect(projection.Color).toEqual(rawProjection.Color);
			expect(projection.Description).toEqual(rawProjection.Description);

			var startInMinute = moment(rawProjection.Start).diff(timeLine.Offset, 'minutes') - timeLine.StartMinute;

			var expectedStart;
			var expectedLength;

			if (startInMinute >= 0) {
				expectedStart = startInMinute * timeLine.LengthPercentPerMinute;
				expectedLength = rawProjection.Minutes * timeLine.LengthPercentPerMinute;
			} else {
				expectedStart = 0;
				expectedLength = (rawProjection.Minutes + startInMinute) * timeLine.LengthPercentPerMinute;
			}

			expect(projection.StartPosition).toEqual(expectedStart);
			expect(projection.Length).toEqual(expectedLength);

		});
	};

	function verifyDayOff(timeLine, dayOff, rawDayOff) {
		expect(dayOff).toBeDefined();
		expect(dayOff.DayOffName).toEqual(rawDayOff.DayOffName);

		var startMinutes = moment(rawDayOff.Start).diff(timeLine.Offset, 'minutes');
		var expectedStartMinutes = startMinutes < timeLine.StartMinute ? 0 : (startMinutes - timeLine.StartMinute);
		expect(dayOff.StartPosition).toEqual(expectedStartMinutes * timeLine.LengthPercentPerMinute);

		var endMinutes = moment(rawDayOff.Start).add(rawDayOff.Minutes, "minute").diff(timeLine.Offset, 'minutes');
		var actualEndMinutes = endMinutes > timeLine.EndMinute ? timeLine.EndMinute : endMinutes;
		var expectedLengthInMinutes = (actualEndMinutes - timeLine.StartMinute) - expectedStartMinutes;
		expect(dayOff.Length).toEqual(expectedLengthInMinutes * timeLine.LengthPercentPerMinute);
	};

});