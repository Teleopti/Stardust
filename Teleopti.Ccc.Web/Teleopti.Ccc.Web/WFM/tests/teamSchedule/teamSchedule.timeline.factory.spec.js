"use strict";

describe("TimeLine", function () {
	var dateTimeFormat = "YYYY-MM-DD HH:mm:ss";

	var target;

	beforeEach(module("wfm.teamSchedule"));

	// Setup the mock service in an anonymous module.
	beforeEach(function () {
		module('currentUserInfoService');
		module(function ($provide) {
			$provide.service('CurrentUserInfo', function () {
				return {
					DefaultTimeZone: "Etc/UTC",
					DateFormatLocale: "en-GB"
				};
			});
		});
	});

	//Getting reference of the mocked service
	var mockCurrentUserInfo;
	beforeEach(inject(function(CurrentUserInfo) {
		mockCurrentUserInfo = CurrentUserInfo;
		moment.locale(mockCurrentUserInfo.DateFormatLocale);
	}));
	

	beforeEach(inject(function (TimeLine) {
		target = TimeLine;
	}));

	it("should display 1 extra hour line when schedule starts or end at hour point", inject(function () {
		var today = "2015-10-26";
		var tomorrow = moment(today).add(1, "days").startOf("days").format("YYYY-MM-DD");

		var now = moment(today + " 07:35:00");

		var schedules = [
			{
				"Date": today,
				"Projection": [
					{
						"Start": today + " 12:00",
						"Minutes": 240
					},
					{
						"Start": today + " 16:00",
						"Minutes": 240 // End = "2015-10-26 20:00"
					}
				],
				"DayOff": null
			},
			{
				"Date": today,
				"Projection": [
					{
						"Start": today + " 08:00",
						"Minutes": 120
					},
					{
						"Start": today + " 10:00",
						"Minutes": 360 // End = "2015-10-26 16:00"
					}
				],
				"DayOff": null
			},
			{
				"Date": tomorrow,
				"Projection": [],
				"DayOff":
				{
					"DayOffName": "Day off",
					"Start": tomorrow + " 00:00",
					"Minutes": 1440
				}
			}
		];

		var timeLine = target.Create(schedules, now);
		
		var expectedStart = 420;
		var expectedEnd = 1260;
		var expectedHourPointsCount = 15;

		expect(timeLine.Offset.format(dateTimeFormat)).toEqual(now.startOf("day").format(dateTimeFormat));
		expect(timeLine.StartMinute).toEqual(expectedStart);
		expect(timeLine.EndMinute).toEqual(expectedEnd);
		expect(timeLine.LengthPercentPerMinute).toEqual(100 / (expectedEnd - expectedStart));

		expect(timeLine.HourPoints.length).toEqual(expectedHourPointsCount);

		var firstHourPoint = timeLine.HourPoints[0];
		expect(firstHourPoint.TimeLabel).toEqual("07:00");
		expect(firstHourPoint.Position()).toEqual(0);

		var lastHourPoint = timeLine.HourPoints[expectedHourPointsCount - 1];
		expect(lastHourPoint.TimeLabel).toEqual("21:00");
		expect(lastHourPoint.Position()).toEqual(100);
	}));

	it("should display same whole hour line as the start or end time when schedule starts or ends not at hour point", inject(function () {
		var today = "2015-10-26";
		var tomorrow = moment(today).add(1, "days").startOf("days").format("YYYY-MM-DD");

		var now = moment(today + " 07:35:00");

		var schedules = [
			{
				"Date": today,
				"Projection": [
					{
						"Start": today + " 12:00",
						"Minutes": 240
					},
					{
						"Start": today + " 16:00",
						"Minutes": 250 // End = "2015-10-26 20:10"
					}
				],
				"DayOff": null
			},
			{
				"Date": today,
				"Projection": [
					{
						"Start": today + " 07:30",
						"Minutes": 120 // End = "2015-10-26 09:30"
					},
					{
						"Start": today + " 10:00",
						"Minutes": 360 // End = "2015-10-26 16:00"
					}
				],
				"DayOff": null
			},
			{
				"Date": tomorrow,
				"Projection": [],
				"DayOff":
				{
					"DayOffName": "Day off",
					"Start": tomorrow + " 00:00",
					"Minutes": 1440
				}
			}
		];

		var timeLine = target.Create(schedules, now);
		
		var expectedStart = 420;
		var expectedEnd = 1260;
		var expectedHourPointsCount = 15;

		expect(timeLine.Offset.format(dateTimeFormat)).toEqual(now.startOf("day").format(dateTimeFormat));
		expect(timeLine.StartMinute).toEqual(expectedStart);
		expect(timeLine.EndMinute).toEqual(expectedEnd);
		expect(timeLine.LengthPercentPerMinute).toEqual(100 / (expectedEnd - expectedStart));

		expect(timeLine.HourPoints.length).toEqual(expectedHourPointsCount);

		var firstHourPoint = timeLine.HourPoints[0];
		expect(firstHourPoint.TimeLabel).toEqual("07:00");
		expect(firstHourPoint.Position()).toEqual(0);

		var lastHourPoint = timeLine.HourPoints[expectedHourPointsCount - 1];
		expect(lastHourPoint.TimeLabel).toEqual("21:00");
		expect(lastHourPoint.Position()).toEqual(100);
	}));
});
