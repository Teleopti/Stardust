﻿describe("teamschedule staffing chart service tests",
	function() {
		var target;
		beforeEach(function () {
			module("wfm.teamSchedule");
		});

		beforeEach(inject(function (TeamScheduleChartService) {
			target = TeamScheduleChartService;
		}));

		it('should configure columns consecutively  when data is  incompletely',
			function() {
				var data = {
					"DataSeries": {
						"Date": "2017-10-15T00:00:00",
						"Time":
						[
							"2017-10-15T07:00:00", "2017-10-15T07:15:00", "2017-10-15T07:30:00", "2017-10-15T07:45:00",
							"2017-10-15T08:00:00",
							"2017-10-15T08:15:00", "2017-10-15T08:30:00", "2017-10-15T08:45:00", "2017-10-15T09:00:00",
							"2017-10-15T09:15:00",
							"2017-10-15T09:30:00", "2017-10-15T09:45:00", "2017-10-15T10:00:00", "2017-10-15T10:15:00",
							"2017-10-15T10:30:00",
							"2017-10-15T10:45:00", "2017-10-15T11:00:00", "2017-10-15T11:15:00", "2017-10-15T11:30:00",
							"2017-10-15T11:45:00",
							"2017-10-15T12:00:00", "2017-10-15T12:15:00", "2017-10-15T12:30:00", "2017-10-15T12:45:00",
							"2017-10-15T13:00:00",
							"2017-10-15T13:15:00", "2017-10-15T13:30:00", "2017-10-15T13:45:00", "2017-10-15T14:00:00",
							"2017-10-15T14:15:00",
							"2017-10-15T14:30:00", "2017-10-15T14:45:00", "2017-10-15T15:00:00", "2017-10-15T15:15:00",
							"2017-10-15T15:30:00",
							"2017-10-15T15:45:00", "2017-10-15T16:00:00", "2017-10-15T16:15:00", "2017-10-15T16:30:00",
							"2017-10-15T16:45:00",
							"2017-10-15T17:00:00", "2017-10-15T17:15:00", "2017-10-15T17:30:00", "2017-10-15T17:45:00",
							"2017-10-15T18:00:00",
							"2017-10-15T18:15:00", "2017-10-15T18:30:00", "2017-10-15T18:45:00"
						],
						"ForecastedStaffing":
						[
							0.0, 0.0, 0.0, 0.0, 2.683, 3.102, 3.66, 4.334, 4.885, 5.723, 6.305, 6.971, 7.61, 8.239, 8.687, 9.097, 9.395,
							9.633, 9.729, 9.804, 9.8, 9.739, 9.647, 9.489, 9.334, 9.135, 8.967, 8.833, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
							0.0,
							0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0
						],
						"UpdatedForecastedStaffing": null,
						"ActualStaffing": null,
						"ScheduledStaffing": [],
						"AbsoluteDifference": [
							0.0, 0.0, 0.0, 0.0, -2.683, -3.102, -3.66, -4.334, -4.885, -5.723, -6.305, -6.971, -7.61, -8.239, -8.687, -9.097,
							-9.395, -9.633, -9.729, -9.804, -9.8, -9.739, -9.647, -9.489, -9.334, -9.135, -8.967, -8.833, 0.0, 0.0, 0.0, 0.0,
							0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0
						]
					},
					"StaffingHasData": true
				};

				var config = target.staffingChartConfig(target.prepareStaffingData(data));
				var flattenedValues = [];
				angular.forEach(config.data.columns[4],
					function(value) {
						flattenedValues.push(value);
					});
				expect(config.data.columns[4].length).toEqual(flattenedValues.length);
			});

	});