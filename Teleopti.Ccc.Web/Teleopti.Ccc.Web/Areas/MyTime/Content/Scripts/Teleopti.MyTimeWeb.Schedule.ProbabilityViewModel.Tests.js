/// <reference path="~/Content/Scripts/qunit.js" />
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Portal.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.ViewModels.js" />

$(document).ready(function () {
	module("Teleopti.MyTimeWeb.Schedule.ProbabilityViewModel");

	var constants = Teleopti.MyTimeWeb.Schedule.Constants;

	var userTexts = {
		"high": "High",
		"low": "Low",
		"probabilityForAbsence": "Probability to get absence:",
		"probabilityForOvertime": "Probability to get overtime:"
	};

	var probabilityNames = ["low", "high"];
	var probabilityLabels = [userTexts.low, userTexts.high];
	var expiredProbabilityCssClass = "probability-expired";

	var createDayViewModel = function () {
		return {
			currentTimeInMinutes: -1,
			setUserNowInMinutes: function (nowInMinutes) { this.currentTimeInMinutes = nowInMinutes; },
			userNowInMinute: function () { return this.currentTimeInMinutes; }
		}
	}

	var boundaries = {
		"lengthPercentagePerMinute": 0.01,
		"probabilityStartMinutes": 240, // 04:00
		"probabilityEndMinutes": 1380, // 23:00
	};

	var baseDate = "2017-02-24";

	test("Will not create absence possibility view model before probability start", function () {
		var rawProbability = {
			"StartTime": baseDate + "T03:00:00",
			"EndTime": baseDate + "T03:15:00",
			"Possibility": Math.round(Math.random())
		};
		var continousPeriods = [
			{
				"startTime": 60, // 01:00
				"endTime": 1380 // 23:00
			}
		];
		var dayViewModel = createDayViewModel();
		var vm = new Teleopti.MyTimeWeb.Schedule.ProbabilityViewModel(rawProbability, constants.absenceProbabilityType,
			boundaries, continousPeriods, userTexts, dayViewModel);

		equal($.isEmptyObject(vm), true);
	});

	test("Will not create absence possibility view model after probability end", function () {
		var rawProbability = {
			"StartTime": baseDate + "T23:15:00",
			"EndTime": baseDate + "T23:30:00",
			"Possibility": Math.round(Math.random())
		};
		var continousPeriods = [
			{
				"startTime": 60, // 01:00
				"endTime": 1380 // 23:00
			}
		];
		var dayViewModel = createDayViewModel();
		var vm = new Teleopti.MyTimeWeb.Schedule.ProbabilityViewModel(rawProbability, constants.absenceProbabilityType,
			boundaries, continousPeriods, userTexts, dayViewModel);

		equal($.isEmptyObject(vm), true);
	});

	test("Create normal absence possibility view model within continous periods", function () {
		var rawProbability = {
			"StartTime": baseDate + "T06:00:00",
			"EndTime": baseDate + "T06:15:00",
			"Possibility": Math.round(Math.random())
		};
		var continousPeriods = [
			{
				"startTime": 60, // 01:00
				"endTime": 1380 // 23:00
			}
		];
		var dayViewModel = createDayViewModel();
		var vm = new Teleopti.MyTimeWeb.Schedule.ProbabilityViewModel(rawProbability, constants.absenceProbabilityType,
			boundaries, continousPeriods, userTexts, dayViewModel);

		var expectedIntervalLength = moment(rawProbability.EndTime).diff(moment(rawProbability.StartTime), "minute");
		var expectedHeightPerIntervalInPercentage = boundaries.lengthPercentagePerMinute * expectedIntervalLength * 100;
		var expectedActualClass = "probability-" + probabilityNames[rawProbability.Possibility];

		equal(vm.styleJson.height, expectedHeightPerIntervalInPercentage + "%");

		// Show before current time
		dayViewModel.setUserNowInMinutes(0);
		equal(vm.cssClass(), expectedActualClass);
		equal(vm.tooltips().indexOf(probabilityLabels[rawProbability.Possibility]) > -1, true);

		// Masked after current time
		dayViewModel.setUserNowInMinutes(420);
		equal(vm.cssClass(), expectedActualClass + " " + expiredProbabilityCssClass);
		equal(vm.tooltips(), "");
	});

	test("Create normal absence possibility view model with length greater than 15 minutes", function () {
		var rawProbability = {
			"StartTime": baseDate + "T06:00:00",
			"EndTime": baseDate + "T06:28:00",
			"Possibility": Math.round(Math.random())
		};
		var continousPeriods = [
			{
				"startTime": 60, // 01:00
				"endTime": 1380 // 23:00
			}
		];
		var dayViewModel = createDayViewModel();
		var vm = new Teleopti.MyTimeWeb.Schedule.ProbabilityViewModel(rawProbability, constants.absenceProbabilityType,
			boundaries, continousPeriods, userTexts, dayViewModel);

		var expectedIntervalLength = moment(rawProbability.EndTime).diff(moment(rawProbability.StartTime), "minute");
		var expectedHeightPerIntervalInPercentage = boundaries.lengthPercentagePerMinute * expectedIntervalLength * 100;
		var expectedActualClass = "probability-" + probabilityNames[rawProbability.Possibility];

		equal(vm.styleJson.height, expectedHeightPerIntervalInPercentage + "%");

		// Will not show by default (Current user time is not set)
		equal(vm.tooltips(), "");

		// Show before current time
		dayViewModel.setUserNowInMinutes(0);
		equal(vm.cssClass(), expectedActualClass);
		equal(vm.tooltips().indexOf(probabilityLabels[rawProbability.Possibility]) > -1, true);

		// Masked after current time
		dayViewModel.setUserNowInMinutes(420);
		equal(vm.cssClass(), expectedActualClass + " " + expiredProbabilityCssClass);
		equal(vm.tooltips(), "");
	});

	test("Create normal absence possibility view model to show horizontal within continous periods", function () {
		var rawProbability = {
			"StartTime": baseDate + "T06:00:00",
			"EndTime": baseDate + "T06:15:00",
			"Possibility": Math.round(Math.random())
		};
		var continousPeriods = [
			{
				"startTime": 60, // 01:00
				"endTime": 1380 // 23:00
			}
		];
		var dayViewModel = createDayViewModel();
		var vm = new Teleopti.MyTimeWeb.Schedule.ProbabilityViewModel(rawProbability, constants.absenceProbabilityType,
			boundaries, continousPeriods, userTexts, dayViewModel, constants.horizontalDirectionLayout);

		var expectedIntervalLength = moment(rawProbability.EndTime).diff(moment(rawProbability.StartTime), "minute");
		var expectedWidthPerIntervalInPercentage = boundaries.lengthPercentagePerMinute * expectedIntervalLength * 100;
		var expectedActualClass = "probability-" + probabilityNames[rawProbability.Possibility];

		equal(vm.styleJson.width, expectedWidthPerIntervalInPercentage + "%");

		// Will not show by default (Current user time is not set)
		equal(vm.tooltips(), "");

		// Show before current time
		dayViewModel.setUserNowInMinutes(0);
		equal(vm.cssClass(), expectedActualClass);
		equal(vm.tooltips().indexOf(probabilityLabels[rawProbability.Possibility]) > -1, true);

		// Masked after current time
		dayViewModel.setUserNowInMinutes(420);
		equal(vm.cssClass(), expectedActualClass + " " + expiredProbabilityCssClass);
		equal(vm.tooltips(), "");
	});

	test("Will create a absence possibility view model never visible between continous periods", function () {
		var rawProbability = {
			"StartTime": baseDate + "T06:00:00",
			"EndTime": baseDate + "T06:15:00",
			"Possibility": Math.round(Math.random())
		};
		var continousPeriods = [
			{
				"startTime": 60,
				"endTime": 300
			},
			{
				"startTime": 600,
				"endTime": 1200
			}
		];
		var dayViewModel = createDayViewModel();
		var vm = new Teleopti.MyTimeWeb.Schedule.ProbabilityViewModel(rawProbability, constants.absenceProbabilityType,
			boundaries, continousPeriods, userTexts, dayViewModel);

		var expectedIntervalLength = moment(rawProbability.EndTime).diff(moment(rawProbability.StartTime), "minute");
		var expectedHeightPerIntervalInPercentage = boundaries.lengthPercentagePerMinute * expectedIntervalLength * 100;
		equal(vm.styleJson.height, expectedHeightPerIntervalInPercentage + "%");

		// Will not show by default (Current user time is not set)
		equal(vm.tooltips(), "");

		// Invisible before current time
		dayViewModel.setUserNowInMinutes(0);

		// Masked after current time
		dayViewModel.setUserNowInMinutes(750);
		equal(vm.tooltips(), "");
	});

	test("Will not create overtime possibility view model before probability start", function () {
		var rawProbability = {
			"StartTime": baseDate + "T03:00:00",
			"EndTime": baseDate + "T03:15:00",
			"Possibility": Math.round(Math.random())
		};
		var continousPeriods = [
			{
				"startTime": 60, // 01:00
				"endTime": 1380 // 23:00
			}
		];
		var dayViewModel = createDayViewModel();
		var vm = new Teleopti.MyTimeWeb.Schedule.ProbabilityViewModel(rawProbability, constants.overtimeProbabilityType,
			boundaries, continousPeriods, userTexts, dayViewModel);

		equal($.isEmptyObject(vm), true);
	});

	test("Will not create overtime possibility view model after probability end", function () {
		var rawProbability = {
			"StartTime": baseDate + "T23:15:00",
			"EndTime": baseDate + "T23:30:00",
			"Possibility": Math.round(Math.random())
		};
		var continousPeriods = [
			{
				"startTime": 60, // 01:00
				"endTime": 1380 // 23:00
			}
		];
		var dayViewModel = createDayViewModel();
		var vm = new Teleopti.MyTimeWeb.Schedule.ProbabilityViewModel(rawProbability, constants.overtimeProbabilityType,
			boundaries, continousPeriods, userTexts, dayViewModel);

		equal($.isEmptyObject(vm), true);
	});

	test("Create normal overtime possibility view model within continous periods", function () {
		var rawProbability = {
			"StartTime": baseDate + "T06:00:00",
			"EndTime": baseDate + "T06:15:00",
			"Possibility": Math.round(Math.random())
		};
		var continousPeriods = [
			{
				"startTime": 60, // 01:00
				"endTime": 1380 // 23:00
			}
		];
		var dayViewModel = createDayViewModel();
		var vm = new Teleopti.MyTimeWeb.Schedule.ProbabilityViewModel(rawProbability, constants.overtimeProbabilityType,
			boundaries, continousPeriods, userTexts, dayViewModel);

		var expectedIntervalLength = moment(rawProbability.EndTime).diff(moment(rawProbability.StartTime), "minute");
		var expectedHeightPerIntervalInPercentage = boundaries.lengthPercentagePerMinute * expectedIntervalLength * 100;
		var expectedActualClass = "probability-" + probabilityNames[rawProbability.Possibility];

		equal(vm.styleJson.height, expectedHeightPerIntervalInPercentage + "%");

		// Will not show by default (Current user time is not set)
		equal(vm.tooltips(), "");

		// Show before current time
		dayViewModel.setUserNowInMinutes(0);
		equal(vm.cssClass(), expectedActualClass);
		equal(vm.tooltips().indexOf(probabilityLabels[rawProbability.Possibility]) > -1, true);

		// Masked after current time
		dayViewModel.setUserNowInMinutes(420);
		equal(vm.cssClass(), expectedActualClass + " " + expiredProbabilityCssClass);
		equal(vm.tooltips(), "");
	});

	test("Will create a normal overtime possibility view model between continous periods", function () {
		var rawProbability = {
			"StartTime": baseDate + "T06:00:00",
			"EndTime": baseDate + "T06:15:00",
			"Possibility": Math.round(Math.random())
		};
		var continousPeriods = [
			{
				"startTime": 60,
				"endTime": 300
			},
			{
				"startTime": 600,
				"endTime": 1200
			}
		];
		var dayViewModel = createDayViewModel();
		var vm = new Teleopti.MyTimeWeb.Schedule.ProbabilityViewModel(rawProbability, constants.overtimeProbabilityType,
			boundaries, continousPeriods, userTexts, dayViewModel);

		var expectedIntervalLength = moment(rawProbability.EndTime).diff(moment(rawProbability.StartTime), "minute");
		var expectedHeightPerIntervalInPercentage = boundaries.lengthPercentagePerMinute * expectedIntervalLength * 100;
		var expectedActualClass = "probability-" + probabilityNames[rawProbability.Possibility];
		equal(vm.styleJson.height, expectedHeightPerIntervalInPercentage + "%");

		// Will not show by default (Current user time is not set)
		equal(vm.tooltips(), "");

		// Show before current time
		dayViewModel.setUserNowInMinutes(0);
		equal(vm.cssClass(), expectedActualClass);
		equal(vm.tooltips().indexOf(probabilityLabels[rawProbability.Possibility]) > -1, true);

		// Masked after current time
		dayViewModel.setUserNowInMinutes(420);
		equal(vm.cssClass(), expectedActualClass + " " + expiredProbabilityCssClass);
		equal(vm.tooltips(), "");
	});
});