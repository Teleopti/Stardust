/// <reference path="~/Content/Scripts/qunit.js" />
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Portal.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.ViewModels.js" />

$(document).ready(function () {
	module("Teleopti.MyTimeWeb.Schedule.ProbabilityViewModel");

	var constants = Teleopti.MyTimeWeb.Common.Constants;

	var userTexts = {
		High: "High",
		Low: "Low",
		ProbabilityToGetAbsenceColon: "Probability to get absence:",
		ProbabilityToGetOvertimeColon: "Probability to get overtime:"
	};

	var probabilityNames = ["low", "high"];
	var probabilityLabels = [userTexts.Low, userTexts.High];
	var expiredProbabilityCssClass = "probability-expired";

	var createDayViewModel = function () {
		return {
			currentTimeInMinutes: -1,
			setUserNowInMinutes: function (nowInMinutes) { this.currentTimeInMinutes = nowInMinutes; },
			userNowInMinute: function () { return this.currentTimeInMinutes; }
		}
	}

	var boundaries = {
		lengthPercentagePerMinute: 0.001,
		timelineStartMinutes: 60,
		probabilityStartMinutes: 240, // 04:00
		probabilityEndMinutes: 1380 // 23:00
	};

	var baseDate = "2017-02-24";

	test("should create normal absence possibility view model", function () {
		Teleopti.MyTimeWeb.Common.TimeFormat = "HH:mm";

		var rawProbability = {
			startTimeMoment: moment(baseDate + "T06:00:00"),
			endTimeMoment: moment(baseDate + "T06:15:00"),
			startTimeInMinutes: 360,
			endTimeInMinutes: 375,
			possibility: Math.round(Math.random())
		};
		var dayViewModel = createDayViewModel();
		var vm = new Teleopti.MyTimeWeb.Schedule.ProbabilityViewModel(rawProbability, constants.probabilityType.absence,
			boundaries, userTexts, dayViewModel);

		var expectedIntervalLength = rawProbability.endTimeMoment.diff(rawProbability.startTimeMoment, "minute");
		var expectedStartPositionInPercentage = ((rawProbability.startTimeInMinutes - boundaries.timelineStartMinutes) * boundaries.lengthPercentagePerMinute * 100).toFixed(2);
		var expectedLengthInPercentage = (boundaries.lengthPercentagePerMinute * expectedIntervalLength * 100).toFixed(2);
		var expectedActualClass = "probability-" + probabilityNames[rawProbability.possibility];

		equal(vm.styleJson.top, expectedStartPositionInPercentage + "%");
		equal(vm.styleJson.height, expectedLengthInPercentage + "%");

		// Show before current time
		dayViewModel.setUserNowInMinutes(0);
		equal(vm.cssClass(), expectedActualClass);
		equal(vm.tooltips().indexOf(probabilityLabels[rawProbability.possibility]) > -1, true);

		// Masked after current time
		dayViewModel.setUserNowInMinutes(420);
		equal(vm.cssClass(), expectedActualClass + " " + expiredProbabilityCssClass);
		equal(vm.tooltips().indexOf("06:00 - 06:15") > -1, true);
	});
});