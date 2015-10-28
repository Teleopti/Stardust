"use strict";

describe("PersonSchedule", function () {
	var dateTimeFormat = "YYYY-MM-DD HH:mm:ss";

	var target;

	beforeEach(module("wfm.teamSchedule"));

	// Setup the mock service in an anonymous module.
	beforeEach(module(function($provide) {
		$provide.value("CurrentUserInfoStub", {
			DefaultTimeZone: "Etc/UTC"
		});
	}));

	beforeEach(inject(function (PersonSchedule) {
		target = PersonSchedule;
	}));

	it("Can get an instance of PersonSchedule factory", inject(function () {
		expect(target).toBeDefined();
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

		var index = 0;
		var shift = personSchedule.Shifts[0];
		angular.forEach(shift.Projections, function (projection) {
			var rawProjection = schedule.Projection[index];
			expect(projection.Color).toEqual(rawProjection.Color);
			expect(projection.Description).toEqual(rawProjection.Description);

			var expectedStart = (moment(rawProjection.Start).diff(timeLine.Offset, 'minutes') - timeLine.StartMinute)
				* timeLine.LengthPercentPerMinute;
			var expectedLength = rawProjection.Minutes * timeLine.LengthPercentPerMinute;
			
			expect(projection.StartPosition()).toEqual(expectedStart);
			expect(projection.Length()).toEqual(expectedLength);

			index++;
		});
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
		
		var dayOff = personSchedule.DayOffs[0];
		expect(dayOff).toBeDefined();
		expect(dayOff.StartPosition()).toEqual(0);
		expect(dayOff.Length()).toEqual((timeLine.EndMinute - timeLine.StartMinute) * timeLine.LengthPercentPerMinute);
	}));
});