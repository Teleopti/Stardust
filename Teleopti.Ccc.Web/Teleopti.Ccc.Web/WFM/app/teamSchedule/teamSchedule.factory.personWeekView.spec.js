"use strict";

describe("teamschedule person week view creator tests", function () {
	var target;

	var mockCurrentUserInfo = {
		CurrentUserInfo: function () {
			return { DefaultTimeZone: "Etc/Utc" };
		}
	};


	beforeEach(function () {
		module("wfm.teamSchedule");
		module(function ($provide) {
			$provide.service('CurrentUserInfo', function () {
				return mockCurrentUserInfo;
			});
		});
	});

	beforeEach(inject(function (PersonScheduleWeekViewCreator) {
		target = PersonScheduleWeekViewCreator;
	}));

	it('should get correct time span for agent schedule day', function () {
		var personSchedule = {
			"PersonId": "e60babbe-29f1-4b61-bba2-9b5e015b2585",
			"Name": "Carlos Oliveira",
			"DaySchedules": [
				{
					"IsTerminated": false,
					"IsDayOff": false,
					"DateTimeSpan": { "StartDateTime": "2016-11-13T22:00:00Z", "EndDateTime": "2016-11-14T09:00:00Z", "LocalStartDateTime": "2016-11-13T22:00:00Z", "LocalEndDateTime": "2016-11-14T09:00:00Z", "LocalDateString": "13/11/2016 - 13/11/2016" },
					"Timezone": { "IanaId": "Asia/Shanghai", "DisplayName": "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi" },
					"Title": "Day",
					"Color": "rgb(255,192,128)",
					"Date": { "Date": "2016-11-14T00:00:00" },
					"DayOfWeek": 1,
					"ContractTimeMinutes": 660.0
				}],
			"ContractTimeMinutes": 660.0
		};

		var result = target.Create([personSchedule]);
		expect(result[0].days[0].summeryTimeSpan).toEqual('10:00 PM (-1) - 9:00 AM');
	});

	it('should create correct week view model with first day of week', function () {
		var personSchedule = {
			"PersonId": "b0e35119-4661-4a1b-8772-9b5e015b2564",
			"Name": "Pierre Baldi",
			"DaySchedules":
				[
					{
						"Date": { "Date": "2018-07-01T00:00:00" }
					},
					{
						"Date": { "Date": "2018-07-02T00:00:00" }
					},
					{
						"Date": { "Date": "2018-07-03T00:00:00" }
					},
					{
						"Date": { "Date": "2018-07-04T00:00:00" }
					},
					{
						"Date": { "Date": "2018-07-05T00:00:00" }
					},
					{
						"Date": { "Date": "2018-07-06T00:00:00" }
					},
					{
						"Date": { "Date": "2018-07-07T00:00:00" }
					}], "ContractTimeMinutes": 2400.0
		};
		var result = target.Create([personSchedule]);
		expect(result[0].firstDayOfWeek).toEqual('2018-07-01');
	});
});