describe('#ShiftEditorViewModelFactory#', function () {
	var target;

	beforeEach(module('wfm.templates', 'wfm.teamSchedule'));
	beforeEach(module(function ($provide) {
		$provide.service('Toggle', function () { return { WfmTeamSchedule_ShowInformationForUnderlyingSchedule_74952: true } });
		$provide.service('CurrentUserInfo', function () {
			return {
				CurrentUserInfo: function () {
					return {
						DefaultTimeZone: "Europe/Berlin",
						DateFormatLocale: "sv-SE"
					};
				}
			};
		});
	}));

	beforeEach(inject(function (ShiftEditorViewModelFactory) {
		target = ShiftEditorViewModelFactory;

	}));

	beforeEach(function () {
		moment.locale('sv');
	});
	afterEach(function () {
		moment.locale('en');
	});

	it('should create time line correctly', function () {
		var viewModel = target.CreateTimeline('2018-05-28', 'Europe/Berlin');
		var intervals = viewModel.Intervals;
		expect(intervals.length).toBe(37);
		expect(intervals.map(function (interval) { return interval.Label; })).toEqual( ["00:00", "01:00", "02:00", "03:00", "04:00", "05:00", "06:00", "07:00", "08:00", "09:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00", "16:00", "17:00", "18:00", "19:00", "20:00", "21:00", "22:00", "23:00", "00:00", "01:00", "02:00", "03:00", "04:00", "05:00", "06:00", "07:00", "08:00", "09:00", "10:00", "11:00", "12:00"]);
		expect(intervals[0].Ticks.filter(function (tick) { return tick.IsHalfHour }).length).toBe(1);
		expect(intervals[0].Ticks.filter(function (tick) { return tick.IsHour }).length).toBe(1);
	});

	it('should create time line correctly on DST', function () {
		var viewModel = target.CreateTimeline('2018-03-25', 'Europe/Berlin');
		var intervals = viewModel.Intervals;
		expect(intervals.length).toBe(36);
		expect(intervals.map(function (interval) { return interval.Label; })).toEqual(["00:00", "01:00", "03:00", "04:00", "05:00", "06:00", "07:00", "08:00", "09:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00", "16:00", "17:00", "18:00", "19:00", "20:00", "21:00", "22:00", "23:00", "00:00", "01:00", "02:00", "03:00", "04:00", "05:00", "06:00", "07:00", "08:00", "09:00", "10:00", "11:00", "12:00"]);
	});

	it('should create schedule correctly', function () {
		var underlyingScheduleSummary = {
			"PersonalActivities": [{ "Description": "Chat", "Start": "2018-05-29 08:00", "End": "2018-05-29 09:00" }],
			"PersonPartTimeAbsences": null,
			"PersonMeetings": null
		};
		var schedule = {
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-05-28",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "E-mail",
				"Start": "2018-05-28 08:00",
				"End": "2018-05-28 10:00",
				"Minutes": 120,
				"IsOvertime": false
			},
			{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#8080c0",
				"Description": "E-mail",
				"Start": "2018-05-28 10:00",
				"End": "2018-05-28 12:00",
				"Minutes": 120,
				"IsOvertime": false
			}],
			"Timezone": { "IanaId": "Europe/Berlin" },
			"UnderlyingScheduleSummary": underlyingScheduleSummary
		};

		var schedule = target.CreateSchedule("2018-05-28", "Europe/Berlin", schedule);
		var shiftLayers = schedule.ShiftLayers;
		expect(schedule.Date).toEqual("2018-05-28");
		expect(schedule.Name).toEqual("Annika Andersson");
		expect(schedule.Timezone).toEqual("Europe/Berlin");
		expect(schedule.HasUnderlyingSchedules).toBe(true);
		expect(schedule.ProjectionTimeRange.Start).toBe("2018-05-28 08:00");
		expect(schedule.ProjectionTimeRange.End).toBe("2018-05-28 12:00");
		expect(schedule.UnderlyingScheduleSummary.PersonalActivities[0].TimeSpan).toBe("08:00 - 09:00");
		expect(schedule.UnderlyingScheduleSummary.PersonalActivities[0].Description).toBe("Chat");

		expect(shiftLayers.length).toEqual(2);
		expect(shiftLayers[0].Description).toEqual('E-mail');
		expect(shiftLayers[0].Start).toEqual("2018-05-28 08:00");
		expect(shiftLayers[0].End).toEqual("2018-05-28 10:00");
		expect(shiftLayers[0].TimeSpan).toEqual("08:00 - 10:00");
		expect(shiftLayers[0].IsOvertime).toEqual(false);
		expect(shiftLayers[0].Minutes).toEqual(120);
		expect(shiftLayers[0].ShiftLayerIds).toEqual(["61678e5a-ac3f-4daa-9577-a83800e49622"]);
		expect(shiftLayers[0].Color).toEqual('#ffffff');
		expect(shiftLayers[0].UseLighterBorder).toEqual(false);
		expect(shiftLayers[1].UseLighterBorder).toEqual(true);
	});

	it('should create shift layers and underlying summary schedule timespan based on timezone', function () {
		var underlyingScheduleSummary = {
			"PersonalActivities": [{ "Description": "Chat", "Start": "2018-05-28 08:00", "End": "2018-05-28 09:00" }],
			"PersonPartTimeAbsences": [{ "Description": "Chat", "Start": "2018-05-28 09:00", "End": "2018-05-28 10:00" }],
			"PersonMeetings": [{ "Description": "Chat", "Start": "2018-05-28 10:00", "End": "2018-05-28 11:00" }]
		};
		var schedule = {
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-05-28",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#000000",
				"Description": "E-mail",
				"Start": "2018-05-28 08:00",
				"Minutes": 120,
				"IsOvertime": false
			}],
			"Timezone": { "IanaId": "Europe/Berlin" },
			"UnderlyingScheduleSummary": underlyingScheduleSummary

		};

		var viewModel = target.CreateSchedule("2018-05-28", "Asia/Hong_Kong", schedule);

		expect(viewModel.UnderlyingScheduleSummary.PersonalActivities[0].TimeSpan).toEqual("14:00 - 15:00");
		expect(viewModel.UnderlyingScheduleSummary.PersonPartTimeAbsences[0].TimeSpan).toEqual("15:00 - 16:00");
		expect(viewModel.UnderlyingScheduleSummary.PersonMeetings[0].TimeSpan).toEqual("16:00 - 17:00");
		expect(viewModel.ProjectionTimeRange.Start).toEqual("2018-05-28 14:00");
		expect(viewModel.ProjectionTimeRange.End).toEqual("2018-05-28 16:00");
		expect(viewModel.ShiftLayers[0].Start).toEqual("2018-05-28 14:00");
		expect(viewModel.ShiftLayers[0].End).toEqual("2018-05-28 16:00");
		expect(viewModel.ShiftLayers[0].TimeSpan).toEqual("14:00 - 16:00");

	});

	it('should create shift layers correctly on DST', function () {
		var schedule = {
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-03-25",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#000000",
				"Description": "E-mail",
				"Start": "2018-03-25 01:00",
				"Minutes": 120,
				"IsOvertime": false
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		};

		var viewModel = target.CreateSchedule("2018-03-25", "Europe/Berlin", schedule);
		expect(viewModel.ShiftLayers[0].Start).toEqual("2018-03-25 01:00");
		expect(viewModel.ShiftLayers[0].End).toEqual("2018-03-25 04:00");
		expect(viewModel.ShiftLayers[0].TimeSpan).toEqual("01:00 - 04:00");
	});

	it('should create shift layers with correct time span for overnight shift', function () {
		var schedule = {
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-05-28",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#000000",
				"Description": "E-mail",
				"Start": "2018-05-28 23:00",
				"Minutes": 120,
				"IsOvertime": false
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		};

		var viewModel = target.CreateSchedule("2018-05-28", "Europe/Berlin", schedule);
		expect(viewModel.ShiftLayers[0].TimeSpan).toEqual("2018-05-28 23:00 - 2018-05-29 01:00");
	});

	describe("in locale en-UK", function () {
		beforeEach(function () { moment.locale('en-UK'); });
		afterEach(function () { moment.locale('en'); });

		it('should create time line correctly', function () {
			var viewModel = target.CreateTimeline('2018-05-28', 'Europe/Berlin');
			var intervals = viewModel.Intervals;
			expect(intervals.length).toBe(37);
			expect(intervals.map(function (interval) { return interval.Label; })).toEqual( ["12:00 AM", "1:00 AM", "2:00 AM", "3:00 AM", "4:00 AM", "5:00 AM", "6:00 AM", "7:00 AM", "8:00 AM", "9:00 AM", "10:00 AM", "11:00 AM", "12:00 PM", "1:00 PM", "2:00 PM", "3:00 PM", "4:00 PM", "5:00 PM", "6:00 PM", "7:00 PM", "8:00 PM", "9:00 PM", "10:00 PM", "11:00 PM", "12:00 AM", "1:00 AM", "2:00 AM", "3:00 AM", "4:00 AM", "5:00 AM", "6:00 AM", "7:00 AM", "8:00 AM", "9:00 AM", "10:00 AM", "11:00 AM", "12:00 PM"]);
		});

		it('should create shift layers with correct time span', function () {
			var schedule = {
				"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
				"Name": "Annika Andersson",
				"Date": "2018-03-25",
				"WorkTimeMinutes": 240,
				"ContractTimeMinutes": 240,
				"Projection": [{
					"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
					"Color": "#000000",
					"Description": "E-mail",
					"Start": "2018-03-25 01:00",
					"Minutes": 120,
					"IsOvertime": false
				}],
				"Timezone": { "IanaId": "Europe/Berlin" }
			};

			var viewModel = target.CreateSchedule("2018-03-25", "Europe/Berlin", schedule);
			expect(viewModel.ShiftLayers[0].TimeSpan).toEqual("1:00 AM - 4:00 AM");
		});
	});
});