describe('#ShiftEditorViewModelFactory#', function () {
	var target;

	beforeEach(module('wfm.templates', 'wfm.teamSchedule'));
	beforeEach(
		module(function ($provide) {
			$provide.service('Toggle', function () {
				return { WfmTeamSchedule_ShowInformationForUnderlyingSchedule_74952: true };
			});
			$provide.service('CurrentUserInfo', function () {
				return {
					CurrentUserInfo: function () {
						return {
							DefaultTimeZone: 'Europe/Berlin',
							DateFormatLocale: 'sv-SE'
						};
					}
				};
			});
		})
	);

	beforeEach(
		inject(function (ShiftEditorViewModelFactory) {
			target = ShiftEditorViewModelFactory;
		})
	);

	beforeEach(function () {
		moment.locale('sv');
	});
	afterEach(function () {
		moment.locale('en');
	});

	it('should create time line correctly based on given timeRange', function () {
		var timezone = 'Europe/Berlin';
		var startTime = moment.tz('2018-05-28', timezone);
		var timeRange = {
			Start: startTime,
			End: startTime
				.clone()
				.add(1, 'days')
				.hours(12)
		};
		var viewModel = target.CreateTimeline('2018-05-28', timezone, timeRange);
		var intervals = viewModel.Intervals;
		expect(intervals.length).toBe(37);
		expect(
			intervals.map(function (interval) {
				return interval.Label;
			})
		).toEqual([
			'00:00',
			'01:00',
			'02:00',
			'03:00',
			'04:00',
			'05:00',
			'06:00',
			'07:00',
			'08:00',
			'09:00',
			'10:00',
			'11:00',
			'12:00',
			'13:00',
			'14:00',
			'15:00',
			'16:00',
			'17:00',
			'18:00',
			'19:00',
			'20:00',
			'21:00',
			'22:00',
			'23:00',
			'00:00',
			'01:00',
			'02:00',
			'03:00',
			'04:00',
			'05:00',
			'06:00',
			'07:00',
			'08:00',
			'09:00',
			'10:00',
			'11:00',
			'12:00'
		]);
		expect(
			intervals[0].Ticks.filter(function (tick) {
				return tick.IsHalfHour;
			}).length
		).toBe(1);
		expect(
			intervals[0].Ticks.filter(function (tick) {
				return tick.IsHour;
			}).length
		).toBe(1);

		startTime = moment.tz('2018-05-28', timezone);
		timeRange = {
			Start: startTime,
			End: startTime.clone().add(1, 'days')
		};
		viewModel = target.CreateTimeline('2018-05-28', timezone, timeRange);
		intervals = viewModel.Intervals;
		expect(intervals.length).toBe(25);
		expect(
			intervals.map(function (interval) {
				return interval.Label;
			})
		).toEqual([
			'00:00',
			'01:00',
			'02:00',
			'03:00',
			'04:00',
			'05:00',
			'06:00',
			'07:00',
			'08:00',
			'09:00',
			'10:00',
			'11:00',
			'12:00',
			'13:00',
			'14:00',
			'15:00',
			'16:00',
			'17:00',
			'18:00',
			'19:00',
			'20:00',
			'21:00',
			'22:00',
			'23:00',
			'00:00'
		]);
	});

	it('should create time line correctly on DST', function () {
		var timezone = 'Europe/Berlin';
		var startTime = moment.tz('2018-03-25', timezone);
		var timeRange = {
			Start: startTime,
			End: startTime
				.clone()
				.add(1, 'days')
				.hours(12)
		};
		var viewModel = target.CreateTimeline('2018-03-25', 'Europe/Berlin', timeRange);
		var intervals = viewModel.Intervals;
		expect(intervals.length).toBe(36);
		expect(
			intervals.map(function (interval) {
				return interval.Label;
			})
		).toEqual([
			'00:00',
			'01:00',
			'03:00',
			'04:00',
			'05:00',
			'06:00',
			'07:00',
			'08:00',
			'09:00',
			'10:00',
			'11:00',
			'12:00',
			'13:00',
			'14:00',
			'15:00',
			'16:00',
			'17:00',
			'18:00',
			'19:00',
			'20:00',
			'21:00',
			'22:00',
			'23:00',
			'00:00',
			'01:00',
			'02:00',
			'03:00',
			'04:00',
			'05:00',
			'06:00',
			'07:00',
			'08:00',
			'09:00',
			'10:00',
			'11:00',
			'12:00'
		]);
	});

	it('should create schedule correctly', function () {
		var underlyingScheduleSummary = {
			PersonalActivities: [{ Description: 'Chat', Start: '2018-05-28 08:00', End: '2018-05-28 09:00' }],
			PersonPartTimeAbsences: null,
			PersonMeetings: null
		};
		var schedule = {
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-05-28',
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'E-mail',
					Start: '2018-05-28 08:00',
					End: '2018-05-28 10:00',
					Minutes: 120,
					IsOvertime: false,
					ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				},
				{
					ShiftLayerIds: ['31678e5a-ac3f-4daa-9577-a83800e49625'],
					Color: '#8080c0',
					Description: 'E-mail',
					Start: '2018-05-28 10:00',
					End: '2018-05-28 12:00',
					Minutes: 120,
					IsOvertime: false,
					ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				},
				{
					ShiftLayerIds: ['11678e5a-ac3f-4daa-9577-a83800e49622', '21678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#8080c0',
					Description: 'E-mail',
					Start: '2018-05-28 12:00',
					End: '2018-05-28 13:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '272e02c8-1a84-4064-9a3b-9b5e015ab3c6',
					TopShiftLayerId: '11678e5a-ac3f-4daa-9577-a83800e49622'
				},
				{
					ShiftLayerIds: ['31678e5a-ac3f-4daa-9577-a83800e49627'],
					Color: '#8080c0',
					Description: 'Lunch',
					Start: '2018-05-28 13:00',
					End: '2018-05-28 14:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c7',
					FloatOnTop: true
				}
			],
			Timezone: { IanaId: 'Asia/Hong_Kong' },
			UnderlyingScheduleSummary: underlyingScheduleSummary
		};

		var schedule = target.CreateSchedule('2018-05-28', 'Europe/Berlin', schedule);
		var shiftLayers = schedule.ShiftLayers;
		expect(schedule.Date).toEqual('2018-05-28');
		expect(schedule.Name).toEqual('Annika Andersson');
		expect(schedule.Timezone).toEqual('Asia/Hong_Kong');
		expect(schedule.HasUnderlyingSchedules).toBe(true);
		expect(schedule.ProjectionTimeRange.Start).toBe('2018-05-28 08:00');
		expect(schedule.ProjectionTimeRange.End).toBe('2018-05-28 14:00');
		expect(schedule.UnderlyingScheduleSummary.PersonalActivities[0].TimeSpan).toBe(
			'2018-05-28 08:00 - 2018-05-28 09:00'
		);
		expect(schedule.UnderlyingScheduleSummary.PersonalActivities[0].Description).toBe('Chat');

		expect(shiftLayers.length).toEqual(4);
		expect(shiftLayers[0].Description).toEqual('E-mail');
		expect(shiftLayers[0].Start).toEqual('2018-05-28 08:00');
		expect(shiftLayers[0].End).toEqual('2018-05-28 10:00');
		expect(shiftLayers[0].TimeSpan).toEqual('2018-05-28 08:00 - 2018-05-28 10:00');
		expect(shiftLayers[0].IsOvertime).toEqual(false);
		expect(shiftLayers[0].Minutes).toEqual(120);
		expect(shiftLayers[0].ShiftLayerIds).toEqual(['61678e5a-ac3f-4daa-9577-a83800e49622']);
		expect(shiftLayers[0].Color).toEqual('#ffffff');
		expect(shiftLayers[0].UseLighterBorder()).toEqual(false);
		expect(shiftLayers[1].UseLighterBorder()).toEqual(true);
		expect(shiftLayers[1].ShowDividedLine).toEqual(true);
		expect(shiftLayers[2].TopShiftLayerId).toEqual('11678e5a-ac3f-4daa-9577-a83800e49622');
		expect(shiftLayers[2].ShowDividedLine).toEqual(false);
		expect(shiftLayers[3].FloatOnTop).toEqual(true);
	});

	it('should create shift layers and underlying summary schedule timespan based on timezone', function () {
		var underlyingScheduleSummary = {
			PersonalActivities: [{ Description: 'Chat', Start: '2018-05-28 08:00', End: '2018-05-28 09:00' }],
			PersonPartTimeAbsences: [{ Description: 'Chat', Start: '2018-05-28 09:00', End: '2018-05-28 10:00' }],
			PersonMeetings: [{ Description: 'Chat', Start: '2018-05-28 10:00', End: '2018-05-28 11:00' }]
		};
		var schedule = {
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-05-28',
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#000000',
					Description: 'E-mail',
					Start: '2018-05-28 08:00',
					Minutes: 120,
					IsOvertime: false
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' },
			UnderlyingScheduleSummary: underlyingScheduleSummary
		};

		var viewModel = target.CreateSchedule('2018-05-28', 'Asia/Hong_Kong', schedule);

		expect(viewModel.UnderlyingScheduleSummary.PersonalActivities[0].TimeSpan).toEqual(
			'2018-05-28 14:00 - 2018-05-28 15:00'
		);
		expect(viewModel.UnderlyingScheduleSummary.PersonPartTimeAbsences[0].TimeSpan).toEqual(
			'2018-05-28 15:00 - 2018-05-28 16:00'
		);
		expect(viewModel.UnderlyingScheduleSummary.PersonMeetings[0].TimeSpan).toEqual(
			'2018-05-28 16:00 - 2018-05-28 17:00'
		);
		expect(viewModel.ProjectionTimeRange.Start).toEqual('2018-05-28 14:00');
		expect(viewModel.ProjectionTimeRange.End).toEqual('2018-05-28 16:00');
		expect(viewModel.ShiftLayers[0].Start).toEqual('2018-05-28 14:00');
		expect(viewModel.ShiftLayers[0].End).toEqual('2018-05-28 16:00');
		expect(viewModel.ShiftLayers[0].TimeSpan).toEqual('2018-05-28 14:00 - 2018-05-28 16:00');
	});

	it('should create shift layers correctly on DST', function () {
		var schedule = {
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-03-25',
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#000000',
					Description: 'E-mail',
					Start: '2018-03-25 01:00',
					End: '2018-03-25 03:00',
					Minutes: 120,
					IsOvertime: false
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		};

		var viewModel = target.CreateSchedule('2018-03-25', 'Europe/Berlin', schedule);
		expect(viewModel.ShiftLayers[0].Start).toEqual('2018-03-25 01:00');
		expect(viewModel.ShiftLayers[0].End).toEqual('2018-03-25 03:00');
		expect(viewModel.ShiftLayers[0].TimeSpan).toEqual('2018-03-25 01:00 - 2018-03-25 03:00');
	});

	it('should splice shift layer', function () {
		var schedule = {
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-08-21',
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#000000',
					Description: 'E-mail',
					Start: '2018-08-21 08:00',
					End: '2018-08-21 09:00',
					Minutes: 60,
					IsOvertime: false
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		};

		var viewModel = target.CreateSchedule('2018-08-21', 'Europe/Berlin', schedule);
		viewModel.SpliceLayer({
			Color: '#fff000',
			Description: 'Phone',
			ActivityId: 'fff78e5a-ac3f-4daa-9577-a83800e49622'
		}, '2018-08-21 07:00', '2018-08-21 08:00', 0);

		expect(viewModel.ShiftLayers.length).toEqual(2);

		expect(viewModel.ShiftLayers[0].Start).toEqual('2018-08-21 07:00');
		expect(viewModel.ShiftLayers[0].End).toEqual('2018-08-21 08:00');
		expect(viewModel.ShiftLayers[0].TimeSpan).toEqual('2018-08-21 07:00 - 2018-08-21 08:00');

		
		viewModel.SpliceLayer({
			Color: '#fff000',
			Description: 'Invoice',
			ActivityId: 'ccc78e5a-ac3f-4daa-9577-a83800e49622'
		}, '2018-08-21 08:00', '2018-08-21 09:00', 1,1);

		expect(viewModel.ShiftLayers.length).toEqual(2);
		expect(viewModel.ShiftLayers[1].ActivityId).toEqual('ccc78e5a-ac3f-4daa-9577-a83800e49622');
		expect(viewModel.ShiftLayers[1].Start).toEqual('2018-08-21 08:00');
		expect(viewModel.ShiftLayers[1].End).toEqual('2018-08-21 09:00');
		expect(viewModel.ShiftLayers[1].TimeSpan).toEqual('2018-08-21 08:00 - 2018-08-21 09:00');
	});

	it('should create shift layers with correct time span for overnight shift', function () {
		var schedule = {
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-05-28',
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#000000',
					Description: 'E-mail',
					Start: '2018-05-28 23:00',
					End: '2018-05-29 01:00',
					Minutes: 120,
					IsOvertime: false
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		};

		var viewModel = target.CreateSchedule('2018-05-28', 'Europe/Berlin', schedule);
		expect(viewModel.ShiftLayers[0].TimeSpan).toEqual('2018-05-28 23:00 - 2018-05-29 01:00');
	});

	it('should create shift layers with correct time span if start and date is not same', function () {
		var schedule = {
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-06-01',
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#000000',
					Description: 'E-mail',
					Start: '2018-06-01 22:00',
					Minutes: 240,
					IsOvertime: false
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		};

		var viewModel = target.CreateSchedule('2018-06-01', 'Asia/Hong_Kong', schedule);
		expect(viewModel.ShiftLayers[0].TimeSpan).toEqual('2018-06-02 04:00 - 2018-06-02 08:00');
	});

	it('should get correct underlying time span if start and date is not same', function () {
		var underlyingScheduleSummary = {
			PersonalActivities: [{ Description: 'Chat', Start: '2018-06-01 22:00', End: '2018-06-01 23:00' }],
			PersonPartTimeAbsences: null,
			PersonMeetings: null
		};
		var schedule = {
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-06-01',
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			UnderlyingScheduleSummary: underlyingScheduleSummary,
			Timezone: { IanaId: 'Europe/Berlin' }
		};

		var viewModel = target.CreateSchedule('2018-06-01', 'Asia/Hong_Kong', schedule);
		expect(viewModel.GetSummaryTimeSpan(viewModel.UnderlyingScheduleSummary.PersonalActivities[0])).toEqual(
			'2018-06-02 04:00 - 2018-06-02 05:00'
		);
	});

	describe('in locale en-UK', function () {
		beforeEach(function () {
			moment.locale('en-UK');
		});
		afterEach(function () {
			moment.locale('en');
		});

		it('should create time line correctly', function () {
			var timezone = 'Europe/Berlin';
			var startTime = moment.tz('2018-05-28', timezone);
			var timeRange = {
				Start: startTime,
				End: startTime
					.clone()
					.add(1, 'days')
					.hours(12)
			};
			var viewModel = target.CreateTimeline('2018-05-28', 'Europe/Berlin', timeRange);
			var intervals = viewModel.Intervals;
			expect(intervals.length).toBe(37);
			expect(
				intervals.map(function (interval) {
					return interval.Label;
				})
			).toEqual([
				'12:00 AM',
				'1:00 AM',
				'2:00 AM',
				'3:00 AM',
				'4:00 AM',
				'5:00 AM',
				'6:00 AM',
				'7:00 AM',
				'8:00 AM',
				'9:00 AM',
				'10:00 AM',
				'11:00 AM',
				'12:00 PM',
				'1:00 PM',
				'2:00 PM',
				'3:00 PM',
				'4:00 PM',
				'5:00 PM',
				'6:00 PM',
				'7:00 PM',
				'8:00 PM',
				'9:00 PM',
				'10:00 PM',
				'11:00 PM',
				'12:00 AM',
				'1:00 AM',
				'2:00 AM',
				'3:00 AM',
				'4:00 AM',
				'5:00 AM',
				'6:00 AM',
				'7:00 AM',
				'8:00 AM',
				'9:00 AM',
				'10:00 AM',
				'11:00 AM',
				'12:00 PM'
			]);
		});

		it('should create shift layers with correct time span', function () {
			var schedule = {
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-03-25',
				WorkTimeMinutes: 240,
				ContractTimeMinutes: 240,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#000000',
						Description: 'E-mail',
						Start: '2018-03-25 01:00',
						End: '2018-03-25 03:00',
						Minutes: 120,
						IsOvertime: false
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			};

			var viewModel = target.CreateSchedule('2018-03-25', 'Europe/Berlin', schedule);
			expect(viewModel.ShiftLayers[0].TimeSpan).toEqual('03/25/2018 1:00 AM - 03/25/2018 3:00 AM');
		});
	});
});
