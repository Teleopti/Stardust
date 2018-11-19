$(document).ready(function() {
	module('Teleopti.MyTimeWeb.Schedule', {
		setup: function() {},
		teardown: function() {}
	});

	var hash = '';
	var constants = Teleopti.MyTimeWeb.Common.Constants;
	var fakeAddRequestViewModel = Teleopti.MyTimeWeb.Schedule.FakeData.fakeAddRequestViewModel;
	var momentWithLocale = function(date) {
		return moment(date).locale('en-gb');
	};
	var basedDate = momentWithLocale(
		Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(this.BaseUtcOffsetInMinutes)
	).format('YYYY-MM-DD');
	var userTexts = Teleopti.MyTimeWeb.Common.GetUserTexts();

	function getFakeScheduleData() {
		return Teleopti.MyTimeWeb.Schedule.FakeData.getFakeScheduleData();
	}

	function getFakeProbabilityData() {
		return Teleopti.MyTimeWeb.Schedule.FakeData.getFakeProbabilityData();
	}

	function fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(formattedDate) {
		return Teleopti.MyTimeWeb.Schedule.FakeData.fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(formattedDate);
	}

	function setupHash() {
		this.hasher = {
			initialized: {
				add: function() {}
			},
			changed: {
				add: function() {}
			},
			init: function() {},
			setHash: function(data) {
				hash = '#' + data;
			}
		};
	}

	test('should show no overtime possibility if the feature is toggle off in fat client', function() {
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.OvertimeProbabilityEnabled = false;

		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.absence;
		week.updateProbabilityData(getFakeProbabilityData());

		equal(week.overtimeProbabilityEnabled(), false);
	});

	test('should show no absence possibility if set to hide probability', function() {
		var fakeScheduleData = getFakeScheduleData();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);

		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.none;

		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate);
		week.updateProbabilityData(fakeProbabilityData);

		week.days().forEach(function(day) {
			equal(day.probabilities().length, 0);
		});
	});

	test('should change url after switching selected probability type', function() {
		setupHash();

		var fakeScheduleData = getFakeScheduleData();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);

		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.none;
		week.nextWeek();

		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate);
		week.updateProbabilityData(fakeProbabilityData);

		week.previousWeek();

		equal(week.selectedProbabilityType, constants.probabilityType.none);

		week.switchProbabilityType(constants.probabilityType.absence);
		equal(
			Teleopti.MyTimeWeb.Portal.ParseHash({ hash: hash }).hash.indexOf(
				'Probability/' + constants.probabilityType.absence
			) > -1,
			true
		);

		week.switchProbabilityType(constants.probabilityType.overtime);
		equal(
			Teleopti.MyTimeWeb.Portal.ParseHash({ hash: hash }).hash.indexOf(
				'Probability/' + constants.probabilityType.overtime
			) > -1,
			true
		);

		week.switchProbabilityType(constants.probabilityType.none);
		equal(
			Teleopti.MyTimeWeb.Portal.ParseHash({ hash: hash }).hash.indexOf(
				'Probability/' + constants.probabilityType.none
			) > -1,
			true
		);
	});

	test('should keep possibility selection for today when changing date', function() {
		setupHash();

		var fakeScheduleData = getFakeScheduleData();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);

		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.absence;
		week.nextWeek();

		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate);
		week.updateProbabilityData(fakeProbabilityData);

		week.previousWeek();

		equal(week.selectedProbabilityType, constants.probabilityType.absence);

		equal(Teleopti.MyTimeWeb.Portal.ParseHash({ hash: hash }).probability, constants.probabilityType.absence);

		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test('should display selected probability label correctly on page', function() {
		var fakeUserTextsInSwedish = {
			ChanceOfGettingAbsenceRequestGranted: 'Chans att f&#229; fr&#229;nvarof&#246;rfr&#229;gan godk&#228;nd:',
			DescriptionColon: 'Beskrivning:',
			Fair: 'M&#246;jlig',
			Good: 'God',
			HideStaffingInfo: 'D&#246;lj bemanningsinfo',
			High: 'H&#246;g',
			LocationColon: 'Lokal:',
			Low: 'L&#229;g',
			Poor: 'D&#229;lig',
			ProbabilityToGetAbsenceColon: 'Sannolikhet att f&#229; fr&#229;nvaro godk&#228;nd:',
			ProbabilityToGetOvertimeColon: 'Sannolikhet att f&#229; &#246;vertid:',
			SeatBookings: 'Sittplatsbokningar',
			ShowAbsenceProbability: 'Visa sannolikhet f&#246;r fr&#229;nvaro',
			ShowOvertimeProbability: 'Visa sannolikhet f&#246;r &#246;vertid',
			StaffingInfo: 'Bemanningsinfo',
			SubjectColon: '&#196;mne:',
			XRequests: '{0} f&#246;rfr&#229;gning(ar)',
			YouHaveNotBeenAllocatedSeat: '{0} har du inte n&#229;gon tilldelad sittplats.'
		};

		setupHash();

		var fakeScheduleData = getFakeScheduleData();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.userTexts = fakeUserTextsInSwedish;
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.absence;

		equal(week.selectedProbabilityType, constants.probabilityType.absence);

		week.switchProbabilityType(constants.probabilityType.overtime);
		equal(week.probabilityLabel(), 'Visa sannolikhet för övertid');
	});

	test('should keep possibility selection for multiple days when changing date', function() {
		setupHash();
		var fakeScheduleData = getFakeScheduleData();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);

		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.overtime;

		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate);
		week.updateProbabilityData(fakeProbabilityData);

		week.previousWeek();

		equal(week.selectedProbabilityType, constants.probabilityType.overtime);
		equal(Teleopti.MyTimeWeb.Portal.ParseHash({ hash: hash }).probability, constants.probabilityType.overtime);
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test('should show absence possibility within schedule time range', function() {
		var fakeScheduleData = getFakeScheduleData();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.absence;

		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate);
		week.updateProbabilityData(fakeProbabilityData);

		// Total 9:30 ~ 16:45 = 29 intervals
		equal(week.days()[0].probabilities().length, 29);
		equal(week.days()[1].probabilities().length, 0);
		for (var i = 0; i < week.days()[0].probabilities().length; i++) {
			if (i > 0) {
				equal(
					week
						.days()[0]
						.probabilities()
						[i].tooltips().length > 0,
					true
				);
			}
		}
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test('should show overtime possibility within timeline range', function() {
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.TimeLine = [
			{
				Time: '00:00:00',
				TimeLineDisplay: '00:00',
				PositionPercentage: 0,
				TimeFixedFormat: null
			},
			{
				Time: '24:00:00',
				TimeLineDisplay: '00:00',
				PositionPercentage: 1,
				TimeFixedFormat: null
			}
		];
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.overtime;
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate);
		week.updateProbabilityData(fakeProbabilityData);

		equal(week.days()[0].probabilities().length, 96);
		equal(week.days()[1].probabilities().length, 0);
		for (var i = 0; i < week.days()[0].probabilities().length; i++) {
			equal(
				week
					.days()[0]
					.probabilities()
					[i].tooltips().length > 0,
				true
			);
		}
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test('should hide absence possibility earlier than now', function() {
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Possibilities = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.absence;
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate);
		week.updateProbabilityData(fakeProbabilityData);

		week.days()[0].userNowInMinute = 750; // 12:30

		equal(week.days()[0].probabilities().length, 29);

		for (var i = 0; i < week.days()[0].probabilities().length; i++) {
			// Schedule started from 09:30, current time is 12:30
			// Then the first (09:30 - 12:30) * 4 = 12 probabilities should be masked
			if (i < 12)
				equal(
					week
						.days()[0]
						.probabilities()
						[i].tooltips().length > 0,
					false
				);
			else
				equal(
					week
						.days()[0]
						.probabilities()
						[i].tooltips().length > 0,
					true
				);
		}
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test('should hide overtime possibility earlier than now', function() {
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Possibilities = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.overtime;
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate);
		week.updateProbabilityData(fakeProbabilityData);

		week.days()[0].userNowInMinute = 750; // 12:30

		equal(week.days()[0].probabilities().length, 29);
		for (var i = 0; i < week.days()[0].probabilities().length; i++) {
			var probability = week.days()[0].probabilities()[i];
			//9:30 ~ 12:30 = 3 * 4 intervals
			if (i < 12) {
				equal(probability.tooltips().length, 0);
				equal(probability.cssClass().indexOf(constants.probabilityClass.expiredProbabilityClass) > -1, true);
			} else {
				equal(probability.cssClass().indexOf(constants.probabilityClass.expiredProbabilityClass), -1);
				equal(probability.tooltips().length > 0, true);
			}
		}
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test('should show no absence possibility for dayoff', function() {
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].IsDayOff = true;
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.absence;
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate);
		week.updateProbabilityData(fakeProbabilityData);

		week.days()[0].userNowInMinute = 0;
		equal(week.days()[0].probabilities().length, 0);
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test('should show no absence possibility for fullday absence', function() {
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].IsFullDayAbsence = true;
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.absence;
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate);
		week.updateProbabilityData(fakeProbabilityData);

		equal(week.days()[0].probabilities().length, 0);
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test('should show overtime possibility for dayoff', function() {
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].IsDayOff = true;
		fakeScheduleData.TimeLine = [
			{
				Time: '08:00:00',
				TimeLineDisplay: '08:00',
				PositionPercentage: 0,
				TimeFixedFormat: null
			},
			{
				Time: '18:00:00',
				TimeLineDisplay: '18:00',
				PositionPercentage: 1,
				TimeFixedFormat: null
			}
		];
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.overtime;
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate);
		week.updateProbabilityData(fakeProbabilityData);

		week.days()[0].userNowInMinute = 0;
		//08:00 ~ 18:00 = 40 intervals - 2margin = 38
		equal(week.days()[0].probabilities().length, 38);
		for (var i = 0; i < week.days()[0].probabilities().length; i++) {
			equal(
				week
					.days()[0]
					.probabilities()
					[i].tooltips().length > 0,
				true
			);
		}
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test('should show overtime possibility based on site open hour', function() {
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].OpenHourPeriod = {
			StartTime: '10:00:00',
			EndTime: '15:00:00'
		};
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.overtime;
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate);
		week.updateProbabilityData(fakeProbabilityData);

		week.days()[0].userNowInMinute = 0;

		equal(week.days()[0].probabilities().length, 20);
		for (var i = 0; i < week.days()[0].probabilities().length; i++) {
			equal(
				week
					.days()[0]
					.probabilities()
					[i].tooltips().length > 0,
				true
			);
		}
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test('should show overtime possibility for dayoff based on intraday open hour', function() {
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].IsDayOff = true;
		fakeScheduleData.Days[0].OpenHourPeriod = {
			StartTime: '10:00:00',
			EndTime: '15:00:00'
		};
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.overtime;
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate);
		week.updateProbabilityData(fakeProbabilityData);

		week.days()[0].userNowInMinute = 0;

		// In this scenario will show prabability based on length of intraday open hour
		// So should be (15 - 10) * 4
		equal(week.days()[0].probabilities().length, 20);
		for (var i = 0; i < week.days()[0].probabilities().length; i++) {
			equal(
				week
					.days()[0]
					.probabilities()
					[i].tooltips().length > 0,
				true
			);
		}
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test('should show overtime possibility for fullday absence', function() {
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].IsFullDayAbsence = true;
		fakeScheduleData.TimeLine = [
			{
				Time: '08:00:00',
				TimeLineDisplay: '08:00',
				PositionPercentage: 0,
				TimeFixedFormat: null
			},
			{
				Time: '18:00:00',
				TimeLineDisplay: '18:00',
				PositionPercentage: 1,
				TimeFixedFormat: null
			}
		];

		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.overtime;
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate);
		week.updateProbabilityData(fakeProbabilityData);

		week.days()[0].userNowInMinute = 0;

		//08:00 ~ 18:00 = 40 intervals - 2margin = 38
		equal(week.days()[0].probabilities().length, 38);
		for (var i = 0; i < week.days()[0].probabilities().length; i++) {
			equal(
				week
					.days()[0]
					.probabilities()
					[i].tooltips().length > 0,
				true
			);
		}
	});

	test('should show correct overtime possibility for cross day schedule', function() {
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].StartTime = momentWithLocale(basedDate)
			.subtract('day', 1)
			.add('hour', 22)
			.format('YYYY-MM-DDTHH:mm:ss');
		fakeScheduleData.TimeLine = [
			{
				Time: '00:00:00',
				TimeLineDisplay: '00:00',
				PositionPercentage: 0,
				TimeFixedFormat: null
			},
			{
				Time: '18:00:00',
				TimeLineDisplay: '18:00',
				PositionPercentage: 1,
				TimeFixedFormat: null
			}
		];

		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.overtime;
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate);
		week.updateProbabilityData(fakeProbabilityData);

		week.days()[0].userNowInMinute = 0;

		//18 * 4 - 1 margin = 71
		equal(week.days()[0].probabilities().length, 71);
		for (var i = 0; i < week.days()[0].probabilities().length; i++) {
			equal(
				week
					.days()[0]
					.probabilities()
					[i].tooltips().length > 0,
				true
			);
		}
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test('should show correct absence possibility for cross day schedule', function() {
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].Periods[0].StartTime = momentWithLocale(basedDate)
			.subtract('day', 1)
			.add('hour', 22)
			.format('YYYY-MM-DDTHH:mm:ss');

		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.absence;
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate);
		week.updateProbabilityData(fakeProbabilityData);

		week.days()[0].userNowInMinute = 0;

		//according to scheduel period 00:00 ~ 16:45 = 16.75 * 4 =
		equal(week.days()[0].probabilities().length, 67);

		for (var i = 0; i < week.days()[0].probabilities().length; i++) {
			equal(
				week
					.days()[0]
					.probabilities()
					[i].tooltips().length > 0,
				true
			);
		}
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test('should show absence possibility for night shift schedule', function() {
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].Periods[0].EndTime = momentWithLocale(basedDate)
			.add('day', 1)
			.add('hour', 2)
			.format('YYYY-MM-DDTHH:mm:ss');
		fakeScheduleData.TimeLine = [
			{
				Time: '09:15:00',
				TimeLineDisplay: '09:15',
				PositionPercentage: 0,
				TimeFixedFormat: null
			},
			{
				Time: '24:00:00',
				TimeLineDisplay: '24:00',
				PositionPercentage: 1,
				TimeFixedFormat: null
			}
		];

		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.absence;
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(week.days()[0].fixedDate);
		week.updateProbabilityData(fakeProbabilityData);

		week.days()[0].userNowInMinute = 0;

		//9:30 ~ 24:00 = 14.5 * 4 = 58
		equal(week.days()[0].probabilities().length, 58);
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test('should apply multiple day probabilities to week view model', function() {
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		equal(week.days().length, 0);
		week.initializeData(getFakeScheduleData());

		week.selectedProbabilityType = constants.probabilityType.absence;
		week.updateProbabilityData(getFakeProbabilityData());

		equal(week.days().length, 3);
		equal(week.days()[0].probabilities().length, 1);
		equal(
			week
				.days()[0]
				.probabilities()[0]
				.cssClass(),
			Teleopti.MyTimeWeb.Common.Constants.probabilityClass.lowProbabilityClass
		);
		equal(
			week
				.days()[0]
				.probabilities()[0]
				.tooltips()
				.indexOf(userTexts.ProbabilityToGetAbsenceColon) > -1,
			true
		);
		equal(week.days()[0].probabilities()[0].styleJson.left != '', true);
		equal(week.days()[0].probabilities()[0].styleJson.width != '', true);

		equal(week.days()[1].probabilities().length, 1);
		equal(week.days()[1].probabilities().length, 1);
		equal(
			week
				.days()[1]
				.probabilities()[0]
				.cssClass(),
			Teleopti.MyTimeWeb.Common.Constants.probabilityClass.highProbabilityClass
		);
		equal(
			week
				.days()[1]
				.probabilities()[0]
				.tooltips()
				.indexOf(userTexts.ProbabilityToGetAbsenceColon) > -1,
			true
		);
		equal(week.days()[1].probabilities()[0].styleJson.left != '', true);
		equal(week.days()[1].probabilities()[0].styleJson.width != '', true);
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test("should not show probability toggle if current week doesn't intercept with 14 upcoming days period", function() {		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.StaffingInfoAvailableDays = 14;

		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);

		week.selectedProbabilityType = constants.probabilityType.absence;
		week.updateProbabilityData(getFakeProbabilityData());
		equal(week.showProbabilityToggle(), true);

		fakeScheduleData.Days[0].FixedDate = momentWithLocale(basedDate)
			.add('day', 15)
			.format('YYYY-MM-DD');
		fakeScheduleData.Days[0].Periods[0].StartTime = momentWithLocale(fakeScheduleData.Days[0].FixedDate)
			.startOf('day')
			.add('hour', 9)
			.add('minute', 30)
			.format('YYYY-MM-DDTHH:mm:ss');
		fakeScheduleData.Days[0].Periods[0].EndTime = momentWithLocale(fakeScheduleData.Days[0].FixedDate)
			.startOf('day')
			.add('hour', 16)
			.add('minute', 45)
			.format('YYYY-MM-DDTHH:mm:ss');

		fakeScheduleData.Days[0].FixedDate = momentWithLocale(basedDate)
			.add('day', 15)
			.format('YYYY-MM-DD');
		fakeScheduleData.Days[1].Periods[0].StartTime = momentWithLocale(fakeScheduleData.Days[1].FixedDate)
			.startOf('day')
			.add('hour', 9)
			.add('minute', 30)
			.format('YYYY-MM-DDTHH:mm:ss');
		fakeScheduleData.Days[1].Periods[0].EndTime = momentWithLocale(fakeScheduleData.Days[1].FixedDate)
			.startOf('day')
			.add('hour', 16)
			.add('minute', 45)
			.format('YYYY-MM-DDTHH:mm:ss');

		week.selectedProbabilityType = constants.probabilityType.none;
		week.initializeData(fakeScheduleData);
		equal(week.showProbabilityToggle(), false);
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test('should show probability toggle if current week is within staffing info availableDays', function() {
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.StaffingInfoAvailableDays = 28;

		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);

		week.selectedProbabilityType = constants.probabilityType.overtime;
		week.updateProbabilityData(getFakeProbabilityData());
		equal(week.showProbabilityToggle(), true);

		fakeScheduleData.Days[0].FixedDate = momentWithLocale(basedDate)
			.add('day', 15)
			.format('YYYY-MM-DD');
		fakeScheduleData.Days[0].Periods[0].StartTime = momentWithLocale(fakeScheduleData.Days[0].FixedDate)
			.startOf('day')
			.add('hour', 9)
			.add('minute', 30)
			.format('YYYY-MM-DDTHH:mm:ss');
		fakeScheduleData.Days[0].Periods[0].EndTime = momentWithLocale(fakeScheduleData.Days[0].FixedDate)
			.startOf('day')
			.add('hour', 16)
			.add('minute', 45)
			.format('YYYY-MM-DDTHH:mm:ss');

		fakeScheduleData.Days[0].FixedDate = momentWithLocale(basedDate)
			.add('day', 15)
			.format('YYYY-MM-DD');
		fakeScheduleData.Days[1].Periods[0].StartTime = momentWithLocale(fakeScheduleData.Days[1].FixedDate)
			.startOf('day')
			.add('hour', 9)
			.add('minute', 30)
			.format('YYYY-MM-DDTHH:mm:ss');
		fakeScheduleData.Days[1].Periods[0].EndTime = momentWithLocale(fakeScheduleData.Days[1].FixedDate)
			.startOf('day')
			.add('hour', 16)
			.add('minute', 45)
			.format('YYYY-MM-DDTHH:mm:ss');

		week.selectedProbabilityType = constants.probabilityType.none;
		week.initializeData(fakeScheduleData);
		equal(week.showProbabilityToggle(), true);
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test('should select hide staffing info option when switching to hide probability', function() {
		var ajax = {
			Ajax: function(options) {
				if (options.url === '../api/ScheduleStaffingPossibility') {
					options.success(getFakeProbabilityData());
				}
			}
		};

		$('body').append("<span data-bind='text: probabilityLabel()' class='probabilityLabel'></span>");

		Teleopti.MyTimeWeb.Schedule.PartialInit(function() {}, function() {}, ajax);

		var fakeScheduleData = getFakeScheduleData();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);

		week.switchProbabilityType(constants.probabilityType.absence);
		week.switchProbabilityType(constants.probabilityType.none);
		equal(userTexts.HideStaffingInfo, $('.probabilityLabel').text());

		$('.probabilityLabel').remove();
	});

	test('should select hide staffing info option when CheckStaffingByIntraday is changed to false from true', function() {
		var ajax = {
			Ajax: function(options) {
				if (options.url === '../api/ScheduleStaffingPossibility') {
					options.success(getFakeProbabilityData());
				}
			}
		};

		$('body').append("<span data-bind='text: probabilityLabel()' class='probabilityLabel'></span>");

		Teleopti.MyTimeWeb.Schedule.PartialInit(function() {}, function() {}, ajax);

		var fakeScheduleData = getFakeScheduleData();
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);

		week.switchProbabilityType(constants.probabilityType.absence);

		fakeScheduleData.CheckStaffingByIntraday = false;

		week.initializeData(fakeScheduleData);

		equal(userTexts.HideStaffingInfo, $('.probabilityLabel').text());

		$('.probabilityLabel').remove();
	});

	test('should not show overtime probability toggle when OvertimeProbability is disabled', function() {
		setupHash();
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.OvertimeProbabilityEnabled = true;
		var week = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		week.initializeData(fakeScheduleData);
		week.selectedProbabilityType = constants.probabilityType.none;
		week.switchProbabilityType(constants.probabilityType.overtime);
		equal(week.overtimeProbabilityEnabled(), true);

		fakeScheduleData.OvertimeProbabilityEnabled = false;
		week.initializeData(fakeScheduleData);
		equal(week.selectedProbabilityType, constants.probabilityType.none);
	});

	test('should not show probability toggle  when OvertimeProbability and AbsenceProbability are disabled', function() {
		setupHash();
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.AbsenceProbabilityEnabled = false;
		fakeScheduleData.OvertimeProbabilityEnabled = false;

		var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		vm.initializeData(fakeScheduleData);

		equal(vm.showProbabilityToggle(), false);
	});

	test('should reserve parameter when fetching data triggered from ReloadScheduleListener', function() {
		var probabilityParamterValue;
		var ajax = {
			Ajax: function(options) {
				if (options.url === '../api/Schedule/FetchWeekData') {
					probabilityParamterValue = options.data.staffingPossiblityType;
				}
			}
		};
		Teleopti.MyTimeWeb.UserInfo.WhenLoaded = function(callback) {
			callback({ WeekStart: 1 });
		};
		var tempFn = Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate;
		Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate = function(date) {
			return date;
		};
		Teleopti.MyTimeWeb.Schedule.PartialInit(function() {}, function() {}, ajax);
		Teleopti.MyTimeWeb.Schedule.SetupViewModel(
			Teleopti.MyTimeWeb.Common.DateTimeDefaultValues,
			Teleopti.MyTimeWeb.Schedule.LoadAndBindData
		);
		var vm = Teleopti.MyTimeWeb.Schedule.Vm();
		var fakeScheduleData = getFakeScheduleData();
		vm.initializeData(fakeScheduleData);
		vm.selectedProbabilityType = constants.probabilityType.overtime;

		var date = momentWithLocale(basedDate).weekday(1);
		Teleopti.MyTimeWeb.Schedule.ReloadScheduleListener({ StartDate: date, EndDate: date });
		equal(probabilityParamterValue, constants.probabilityType.overtime);

		Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate = tempFn;
	});
});
