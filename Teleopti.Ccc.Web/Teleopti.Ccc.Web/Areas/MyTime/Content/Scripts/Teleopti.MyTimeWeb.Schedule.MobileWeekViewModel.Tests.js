/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="~/Areas/MyTime/Content/Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel.js" />
/// <reference path="~/Content/Scripts/qunit.js" />

$(document).ready(function () {
	module("Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel");

	var createTimeline = function (startHour, endHour) {
		var timelinePoints = [];

		if (startHour > 0) {
			timelinePoints.push({
				"Time": (startHour - 1) + ":45:00",
				"TimeLineDisplay": (startHour - 1) + ":45"
			});
		}

		for (var i = startHour; i <= endHour; i++) {
			timelinePoints.push({
				"Time": i + ":00:00",
				"TimeLineDisplay": i + ":00"
			});
		}

		if (endHour < 24) {
			timelinePoints.push({
				"Time": endHour + ":45:00",
				"TimeLineDisplay": endHour + ":45"
			});
		}

		return timelinePoints;
	}

	test("should read absence report permission", function () {
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel();

		vm.readData({
			PeriodSelection: [{ Display: null }],
			Days: [{}],
			RequestPermission:
			{
				AbsenceReportPermission: true
			}
		});

		equal(vm.absenceReportPermission(), true);
		equal(vm.dayViewModels()[0].absenceReportPermission(), true);
	});

	test("should read scheduled days", function () {
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel();

		vm.readData({
			PeriodSelection: [{
				Display: null
			}],
			Days: [{
			}]
		});

		equal(vm.dayViewModels().length, 1);
	});

	test("Should read timelines", function () {
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel();

		var timelineStart = 8;
		var timelineEnd = 16;
		var lengthInHour = timelineEnd - timelineStart + 1;
		var rawTimeline = createTimeline(timelineStart, timelineEnd);

		var rawData = {
			PeriodSelection: [{
				Display: null
			}],
			Days: [{}],
			TimeLine: rawTimeline
		};

		vm.readData(rawData);

		var timelines = vm.timeLines();
		equal(timelines.length, lengthInHour + 2); // 2 extra timeline point will be created
		equal(timelines[0].minutes, timelineStart * 60 - 15);
		equal(timelines[timelines.length - 1].minutes, timelineEnd * 60 + 45);
	});

	test("Should show the staffing probability for today", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
			if (x === "MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880") return false;
		};
		
		var vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel();
		var fakeData = {
			PeriodSelection: [{ Display: null }],
			BaseUtcOffsetInMinutes: 60,
			Days: [
			{
				FixedDate: moment(Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(this.BaseUtcOffsetInMinutes)).format('YYYY-MM-DD')
			}],
			ViewPossibilityPermission: true
		};

		vm.readData(fakeData);
		equal(vm.dayViewModels()[0].showProbabilityOptions(), true);
	});

	test("Should not show probability option model if it is not current day when MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880 is off", function() {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
			if (x === "MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880") return false;
		};

		var fakeData = {
			PeriodSelection: [{
				Display: null
			}],
			BaseUtcOffsetInMinutes: 60,
			Days: [{
					FixedDate: moment(Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(this.BaseUtcOffsetInMinutes)).format('YYYY-MM-DD')
				},
				{
					FixedDate: moment(Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(this.BaseUtcOffsetInMinutes)).add('day', 1).format('YYYY-MM-DD')
				}],
			Possibilities: [],
			ViewPossibilityPermission: true
		};

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel();
		vm.readData(fakeData);

		var fakeFixedDateObj = {fixedDate: function(){return fakeData.Days[0].FixedDate}};
		vm.toggleProbabilityOptionsPanel(fakeFixedDateObj);
		equal(vm.dayViewModels()[0].showProbabilityOptions(), true);
		equal(vm.requestViewModel() != null, true);
		equal(vm.dayViewModels()[0].isModelVisible(), true);

		vm.toggleProbabilityOptionsPanel(fakeFixedDateObj);
		equal(vm.requestViewModel() == null, true);
		equal(vm.dayViewModels()[1].showProbabilityOptions(), false);
		equal(vm.dayViewModels()[1].isModelVisible(), false);
	});

	test("Should show the staffing probability for up to 14 upcoming days", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
			if (x === "MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880") return true;
		};

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel();

		vm.readData({
			PeriodSelection: [{ Display: null }],
			BaseUtcOffsetInMinutes: 60,
			Days: [{
					FixedDate: moment(Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(this.BaseUtcOffsetInMinutes)).format('YYYY-MM-DD')
				},
				{
					FixedDate: moment(Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(this.BaseUtcOffsetInMinutes)).add('day',1).format('YYYY-MM-DD')
				}],
			Possibilities: [],
			ViewPossibilityPermission: true
		});

		equal(vm.dayViewModels()[0].showProbabilityOptions(), false);
		equal(vm.dayViewModels()[1].showProbabilityOptions(), false);
		equal(vm.showProbabilityOptionsToggleIcon(), true);
	});

	test("Should toggle the global staffing probability option form after clicking the toggle icon when 'MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880'", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
			if (x === "MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880") return true;
		};

		var fakeData = {
			PeriodSelection: [{ Display: null }],
			BaseUtcOffsetInMinutes: 60,
			Days: [{
					FixedDate: moment(Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(this.BaseUtcOffsetInMinutes)).format('YYYY-MM-DD')
				},
				{
					FixedDate: moment(Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(this.BaseUtcOffsetInMinutes)).add('day',1).format('YYYY-MM-DD')
				}],
			Possibilities: [],
			ViewPossibilityPermission: true
		};

		var vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel();
		vm.readData(fakeData);

		var fakeFixedDateObj = {fixedDate: function(){return fakeData.Days[0].FixedDate}};

		equal(vm.dayViewModels()[0].showProbabilityOptions(), false);
		equal(vm.dayViewModels()[1].showProbabilityOptions(), false);
		equal(vm.showProbabilityOptionsToggleIcon(), true);
		equal(vm.showProbabilityOptionsForm(), false);

		vm.toggleProbabilityOptionsPanel(fakeFixedDateObj);
		equal(vm.showProbabilityOptionsForm(), true);

		vm.toggleProbabilityOptionsPanel(fakeFixedDateObj);
		equal(vm.showProbabilityOptionsForm(), false);
	});

	
	test("Should show probability data for multiple upcoming days when MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880 is on", function() {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function(x) {
			if (x === "MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913") return true;
			if (x === "MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880") return true;
		};
		var fakeData = {
			PeriodSelection: [{
				Display: null
			}],
			BaseUtcOffsetInMinutes: 60,
			Days: [{
					FixedDate: moment(Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(this.BaseUtcOffsetInMinutes)).format('YYYY-MM-DD'),
					Periods: [{
						"Title": "Phone",
						"TimeSpan": "09:30 - 16:45",
						"StartTime": moment(Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(this.BaseUtcOffsetInMinutes)).startOf('day').add('hour', 9).add('minute', 30).format('YYYY-MM-DDTHH:mm:ss'),
						"EndTime": moment(Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(this.BaseUtcOffsetInMinutes)).startOf('day').add('hour', 16).add('minute', 45).format('YYYY-MM-DDTHH:mm:ss'),
						"Summary": "7:15",
						"StyleClassName": "color_80FF80",
						"Meeting": null,
						"StartPositionPercentage": 0.1896551724137931034482758621,
						"EndPositionPercentage": 1,
						"Color": "128,255,128",
						"IsOvertime": false
					}]
				},
				{
					FixedDate: moment(Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(this.BaseUtcOffsetInMinutes)).add('day', 1).format('YYYY-MM-DD'),
					Periods: [{
						"Title": "Phone",
						"TimeSpan": "09:30 - 16:45",
						"StartTime": moment(Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(this.BaseUtcOffsetInMinutes)).startOf('day').add('day', 1).add('hour', 9).add('minute', 30).format('YYYY-MM-DDTHH:mm:ss'),
						"EndTime": moment(Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(this.BaseUtcOffsetInMinutes)).startOf('day').add('day', 1).add('hour', 16).add('minute', 45).format('YYYY-MM-DDTHH:mm:ss'),
						"Summary": "7:15",
						"StyleClassName": "color_80FF80",
						"Meeting": null,
						"StartPositionPercentage": 0.1896551724137931034482758621,
						"EndPositionPercentage": 1,
						"Color": "128,255,128",
						"IsOvertime": false
					}]
				}],
			Possibilities: [
				{
					Date: moment(Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(this.BaseUtcOffsetInMinutes)).format('YYYY-MM-DD'),
					StartTime: moment(Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(this.BaseUtcOffsetInMinutes)).startOf('day').format('YYYY-MM-DDTHH:mm:ss'),
					EndTime: moment(Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(this.BaseUtcOffsetInMinutes)).startOf('day').add('hour', 16).add('minute', 45).format('YYYY-MM-DDTHH:mm:ss'),
					Possibility: 0
				}, {
					Date: moment(Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(this.BaseUtcOffsetInMinutes)).add('day', 1).format('YYYY-MM-DD'),
					StartTime: moment(Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(this.BaseUtcOffsetInMinutes)).startOf('day').add('day', 1).format('YYYY-MM-DDTHH:mm:ss'),
					EndTime: moment(Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(this.BaseUtcOffsetInMinutes)).startOf('day').add('day', 1).add('hour', 16).add('minute', 45).format('YYYY-MM-DDTHH:mm:ss'),
					Possibility: 1
				}
			],
			ViewPossibilityPermission: true,
			TimeLine:[{Time: "06:45:00", TimeLineDisplay: "06:45", PositionPercentage: 0, TimeFixedFormat: null},
					  {Time: "16:45:00", TimeLineDisplay: "16:45", PositionPercentage: 1, TimeFixedFormat: null}]
		};

		var fakeUserText = {
			probabilityForAbsence: '@Resources.ProbabilityToGetAbsenceColon',
			probabilityForOvertime: '@Resources.ProbabilityToGetOvertimeColon',
			hideStaffingInfo: '@Resources.HideStaffingInfo',
			showAbsenceProbability: '@Resources.ShowAbsenceProbability',
			showOvertimeProbability: '@Resources.ShowOvertimeProbability'
		};

		var vm;
		var fakeReloadData = function(){
			vm.readData(fakeData);
		};

		var fakeFixedDateObj = {fixedDate: function(){return fakeData.Days[0].FixedDate}};

		vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(fakeUserText, null, fakeReloadData);
		vm.toggleProbabilityOptionsPanel(fakeFixedDateObj);
		vm.OnProbabilityOptionSelectCallback(Teleopti.MyTimeWeb.Common.Constants.probabilityType.overtime);

		equal(vm.dayViewModels()[0].probabilities().length, 1);
		equal(vm.dayViewModels()[0].probabilities()[0].cssClass(), Teleopti.MyTimeWeb.Common.Constants.probabilityClass.lowProbabilityClass);
		equal(vm.dayViewModels()[0].probabilities()[0].tooltips().indexOf(fakeUserText.probabilityForOvertime) > -1, true);
		equal(vm.dayViewModels()[0].probabilities()[0].styleJson.left != '', true);
		equal(vm.dayViewModels()[0].probabilities()[0].styleJson.width != '', true);

		equal(vm.dayViewModels()[1].probabilities().length, 1);
		equal(vm.dayViewModels()[1].probabilities()[0].cssClass(), Teleopti.MyTimeWeb.Common.Constants.probabilityClass.highProbabilityClass);
		equal(vm.dayViewModels()[1].probabilities()[0].tooltips().indexOf(fakeUserText.probabilityForOvertime) > -1, true);
		equal(vm.dayViewModels()[1].probabilities()[0].styleJson.left != '', true);
		equal(vm.dayViewModels()[1].probabilities()[0].styleJson.width != '', true);
	});
});