/// <reference path="~/Content/moment/moment.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.js" />

if (typeof (Teleopti) === "undefined") {
	Teleopti = {};

	if (typeof (Teleopti.MyTimeWeb) === "undefined") {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.Schedule.Helper = (function ($) {
	var constants = Teleopti.MyTimeWeb.Schedule.Constants;

	var getContinousPeriods = function (baseDate, periods) {
		if (!periods || periods.length === 0) return [];

		var continousPeriods = [];
		var previousEndMinutes = 0;
		var continousPeriodStart = 0;
		for (var i = 0; i < periods.length; i++) {
			var currentPeriodStartMinutes = moment(periods[i].StartTime).diff(baseDate) / (60 * 1000);
			var currentPeriodEndMinutes = moment(periods[i].EndTime).diff(baseDate) / (60 * 1000);

			if (currentPeriodStartMinutes < 0) {
				currentPeriodStartMinutes = 0;
			}

			if (currentPeriodEndMinutes > constants.totalMinutesOfOneDay) {
				currentPeriodEndMinutes = constants.totalMinutesOfOneDay;
			}

			if (currentPeriodStartMinutes === currentPeriodEndMinutes) continue;

			if (i === 0) {
				continousPeriodStart = currentPeriodStartMinutes;
			}

			if (previousEndMinutes !== 0 && currentPeriodStartMinutes !== previousEndMinutes) {
				continousPeriods.push({
					"startTime": continousPeriodStart,
					"endTime": previousEndMinutes
				});
				continousPeriodStart = currentPeriodStartMinutes;
			}

			if (i === periods.length - 1) {
				continousPeriods.push({
					"startTime": continousPeriodStart,
					"endTime": currentPeriodEndMinutes
				});
			}
			previousEndMinutes = currentPeriodEndMinutes;
		}

		return continousPeriods;
	};

	var createProbabilityModels = function (scheduleDay, rawProbabilities, dayViewModel, options) {
		if (!options.staffingProbabilityEnabled || rawProbabilities == undefined || rawProbabilities.length === 0) {
			return [];
		}

		// If today is full day absence or dayoff, Then hide absence probabilities
		if (options.probabilityType === constants.noneProbabilityType ||
			(options.probabilityType === constants.absenceProbabilityType && (scheduleDay.IsFullDayAbsence || scheduleDay.IsDayOff))) {
			return [];
		}

		var continousPeriods = [];

		var date = moment(scheduleDay.FixedDate);
		if (options.probabilityType === constants.absenceProbabilityType) {
			continousPeriods = Teleopti.MyTimeWeb.Schedule.Helper.GetContinousPeriods(date, scheduleDay.Periods);
		}

		var boundaries = new Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary(scheduleDay, options.timelines,
			options.probabilityType, rawProbabilities, options.intradayOpenPeriod);

		var styleJson = {};
		var lengthProperty = options.layoutDirection === constants.horizontalDirectionLayout ? "width" : "height";
		styleJson[lengthProperty] = (boundaries.probabilityStartPosition * 100) + "%";

		var probabilitieModels = [];
		// Add an "invisible" probability on top to make all probabilities displayed from correct position
		probabilitieModels.push({
			actualClass: "probability-none",
			actualTooltips: "",
			cssClass: function () { return "probability-none"; },
			tooltips: function () { return "" },
			styleJson: styleJson
		});

		for (var i = 0; i < rawProbabilities.length; i++) {
			var probabilityModel = new Teleopti.MyTimeWeb.Schedule.ProbabilityViewModel(rawProbabilities[i],
				options.probabilityType, boundaries, continousPeriods, options.userTexts, dayViewModel, options.layoutDirection);
			if (!$.isEmptyObject(probabilityModel)) {
				probabilitieModels.push(probabilityModel);
			}
		}

		return probabilitieModels;
	}

	return {
		GetContinousPeriods: getContinousPeriods,
		CreateProbabilityModels: createProbabilityModels
	};
})(jQuery);