describe('requestScheduleService', function() {
	var requestScheduleService;
	beforeEach(module('wfm.requests'));
	beforeEach(inject(function($injector) {
		requestScheduleService = $injector.get('requestScheduleService');
	}));

	it('should build shift data', function() {
		//The timezone in the data from backend is loggger user/admin's timezone: Europe/Berlin
		var shift = {
			Name: 'Ashley Andeen',
			Periods: [
				{
					Title: 'Social Media',
					TimeSpan: '7:00 PM - 9:15 PM',
					Color: '30,144,255',
					StartTime: '2018-11-23T19:00:00',
					EndTime: '2018-11-23T21:15:00',
					IsOvertime: false,
					StartPositionPercentage: 0.0,
					EndPositionPercentage: 0.25,
					Meeting: {
						Title: 'Meeting Title',
						Description: 'Meeting Description',
						Location: 'Meeting Location'
					}
				},
				{
					Title: 'Short break',
					TimeSpan: '9:15 PM - 9:30 PM',
					Color: '255,0,0',
					StartTime: '2018-11-23T21:15:00',
					EndTime: '2018-11-23T21:30:00',
					IsOvertime: false,
					StartPositionPercentage: 0.25,
					EndPositionPercentage: 0.2778,
					Meeting: null
				},
				{
					Title: 'Social Media',
					TimeSpan: '9:30 PM - 11:30 PM',
					Color: '30,144,255',
					StartTime: '2018-11-23T21:30:00',
					EndTime: '2018-11-23T23:30:00',
					IsOvertime: false,
					StartPositionPercentage: 0.2778,
					EndPositionPercentage: 0.5,
					Meeting: null
				},
				{
					Title: 'Lunch',
					TimeSpan: '11:30 PM - 12:30 AM',
					Color: '255,255,0',
					StartTime: '2018-11-23T23:30:00',
					EndTime: '2018-11-24T00:30:00',
					IsOvertime: false,
					StartPositionPercentage: 0.5,
					EndPositionPercentage: 0.6111,
					Meeting: null
				},
				{
					Title: 'Social Media',
					TimeSpan: '12:30 AM - 2:15 AM',
					Color: '30,144,255',
					StartTime: '2018-11-24T00:30:00',
					EndTime: '2018-11-24T02:15:00',
					IsOvertime: false,
					StartPositionPercentage: 0.6111,
					EndPositionPercentage: 0.8056,
					Meeting: null
				},
				{
					Title: 'Short break',
					TimeSpan: '2:15 AM - 2:30 AM',
					Color: '255,0,0',
					StartTime: '2018-11-24T02:15:00',
					EndTime: '2018-11-24T02:30:00',
					IsOvertime: false,
					StartPositionPercentage: 0.8056,
					EndPositionPercentage: 0.8333,
					Meeting: null
				},
				{
					Title: 'Social Media',
					TimeSpan: '2:30 AM - 4:00 AM',
					Color: '30,144,255',
					StartTime: '2018-11-24T02:30:00',
					EndTime: '2018-11-24T04:00:00',
					IsOvertime: false,
					StartPositionPercentage: 0.8333,
					EndPositionPercentage: 1.0,
					Meeting: null
				}
			],
			IsDayOff: false,
			DayOffName: null,
			IsNotScheduled: false,
			ShiftCategory: {
				Id: null,
				ShortName: 'PM',
				Name: 'Late',
				DisplayColor: '#000000'
			},
			BelongsToDate: '2018-11-23T00:00:00'
		};

		var shiftData = requestScheduleService.buildShiftData(shift, 'Europe/Berlin', 'Asia/Amman');

		expect(shiftData).toBeTruthy();
		expect(shiftData.Name).toBe('Ashley Andeen');
		expect(shiftData.Date).toBe('11/23/18');
		expect(shiftData.Periods.length).toBe(7);
		expect(shiftData.Periods[0].Title).toBe('Social Media');
		expect(shiftData.Periods[0].TimeSpan).toBe('8:00 PM - 10:15 PM');
		expect(shiftData.Periods[0].Color).toBe('rgb(30,144,255)');
		expect(shiftData.Periods[0].Meeting.Title).toBe('Meeting Title');
		expect(shiftData.Periods[0].Meeting.Description).toBe('Meeting Description');
		expect(shiftData.Periods[0].Meeting.Location).toBe('Meeting Location');
		expect(shiftData.Periods[0].StartTime).toBe('2018-11-23T20:00:00');
		expect(shiftData.Periods[0].EndTime).toBe('2018-11-23T22:15:00');
		expect(shiftData.Periods[0].IsOvertime).toBe(false);
		expect(shiftData.Periods[0].StartPositionPercentage).toBe(0.0);
		expect(shiftData.Periods[0].EndPositionPercentage).toBe(0.25);
		expect(shiftData.Periods[shiftData.Periods.length - 1].TimeSpan).toBe('3:30 AM - 5:00 AM');
		expect(shiftData.IsDayOff).toBe(false);
		expect(shiftData.IsNotScheduled).toBe(false);
		expect(shiftData.DayOffName).toBe(null);
		expect(shiftData.ShiftCategory.Id).toBe(null);
		expect(shiftData.ShiftCategory.ShortName).toBe('PM');
		expect(shiftData.ShiftCategory.Name).toBe('Late');
		expect(shiftData.ShiftCategory.DisplayColor).toBe('#000000');
		expect(shiftData.ShiftCategory.TextColor).toBe('white');
		expect(shiftData.ShiftStartTime).toBe('8:00 PM');
		expect(shiftData.ShiftEndTime).toBe('5:00 AM+1');
	});

	it('should build shift data for full day absence request', function() {
		//The timezone in the data from backend is loggger user/admin's timezone: Europe/Berlin
		var shift = {
			Name: 'Ashley Andeen',
			Periods: [
				{
					Title: 'Illness',
					TimeSpan: '8:00 AM - 5:00 PM',
					Color: '255,0,0',
					StartTime: '2018-11-30T08:00:00',
					EndTime: '2018-11-30T17:00:00',
					IsOvertime: false,
					StartPositionPercentage: 0.0,
					EndPositionPercentage: 1.0,
					Meeting: null
				}
			],
			IsDayOff: false,
			DayOffName: null,
			IsNotScheduled: false,
			ShiftCategory: null,
			BelongsToDate: '2018-11-30T00:00:00'
		};

		var shiftData = requestScheduleService.buildShiftData(shift, 'Europe/Berlin', 'Europe/Berlin');

		expect(shiftData).toBeTruthy();
		expect(shiftData.Name).toBe('Ashley Andeen');
		expect(shiftData.Date).toBe('11/30/18');
		expect(shiftData.Periods.length).toBe(1);
		expect(shiftData.Periods[0].Title).toBe('Illness');
		expect(shiftData.Periods[0].TimeSpan).toBe('8:00 AM - 5:00 PM');
		expect(shiftData.Periods[0].Color).toBe('rgb(255,0,0)');
		expect(shiftData.Periods[0].Meeting).toBe(null);
		expect(shiftData.Periods[0].StartTime).toBe('2018-11-30T08:00:00');
		expect(shiftData.Periods[0].EndTime).toBe('2018-11-30T17:00:00');
		expect(shiftData.Periods[0].IsOvertime).toBe(false);
		expect(shiftData.Periods[0].StartPositionPercentage).toBe(0.0);
		expect(shiftData.Periods[0].EndPositionPercentage).toBe(1.0);
		expect(shiftData.IsDayOff).toBe(false);
		expect(shiftData.IsNotScheduled).toBe(false);
		expect(shiftData.DayOffName).toBe(null);
		expect(shiftData.ShiftCategory).toBe(null);
		expect(shiftData.ShiftStartTime).toBe('8:00 AM');
		expect(shiftData.ShiftEndTime).toBe('5:00 PM');
	});
});
