"use strict";

describe("TimeLine", function () {
	var dateTimeFormat = "YYYY-MM-DD HH:mm:ss";

	var target;

	beforeEach(module("wfm.teamSchedule"));

	// Setup the mock service in an anonymous module.
	beforeEach(module(function ($provide) {
		$provide.value("CurrentUserInfoStub", {
			DefaultTimeZone: "Utc/Utc"
		});
	}));

	beforeEach(inject(function(TimeLine) {
		target = TimeLine;
	}));

	it("Can get an instance of TimeLine factory", inject(function () {
		expect(target).toBeDefined();
	}));

	it("Can get correct TimeLine", inject(function () {
		var now = moment("2015-10-26 07:35:00");

		var schedules = [
			{
				"Date": "2015-10-26",
				"Projection": [
					{
						"Start": "2015-10-26 12:00",
						"Minutes": 240
					},
					{
						"Start": "2015-10-26 16:00",
						"Minutes": 240 // End = "2015-10-26 20:00"
					}
				],
				"DayOff": null
			},
			{
				"Date": "2015-10-26",
				"Projection": [
					{
						"Start": "2015-10-26 08:00",
						"Minutes": 120
					},
					{
						"Start": "2015-10-26 10:00",
						"Minutes": 360 // End = "2015-10-26 16:00"
					}
				],
				"DayOff": null
			},
			{
				"Date": "2015-10-27",
				"Projection": [],
				"DayOff":
				{
					"DayOffName": "Day off",
					"Start": "2015-10-27 00:00",
					"Minutes": 1440
				}
			}
		];

		var timeLine = target.Create(schedules, now);
		
		var expectedStart = 480;
		var expectedEnd = 1200;
		var expectedHourPointsCount = 13;

		expect(timeLine.Offset.format(dateTimeFormat)).toEqual(now.startOf("day").format(dateTimeFormat));
		expect(timeLine.StartMinute).toEqual(expectedStart);
		expect(timeLine.EndMinute).toEqual(expectedEnd);
		expect(timeLine.LengthPercentPerMinute).toEqual(100 / (expectedEnd - expectedStart));

		expect(timeLine.HourPoints.length).toEqual(expectedHourPointsCount);

		var firstHourPoint = timeLine.HourPoints[0];
		expect(firstHourPoint.TimeLabel).toEqual("08:00");
		expect(firstHourPoint.Position()).toEqual(0);

		var lastHourPoint = timeLine.HourPoints[expectedHourPointsCount - 1];
		expect(lastHourPoint.TimeLabel).toEqual("20:00");
		expect(lastHourPoint.Position()).toEqual(100);
	}));
});
