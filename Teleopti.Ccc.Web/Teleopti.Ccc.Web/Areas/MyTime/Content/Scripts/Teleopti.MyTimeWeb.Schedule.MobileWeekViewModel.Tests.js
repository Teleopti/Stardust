/// <reference path="Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel.js" />
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
});