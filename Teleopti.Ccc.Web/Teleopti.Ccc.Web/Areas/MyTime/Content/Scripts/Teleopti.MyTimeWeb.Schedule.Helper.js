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
					"startTimeInMin": continousPeriodStart,
					"endTimeInMin": previousEndMinutes
				});
				continousPeriodStart = currentPeriodStartMinutes;
			}

			if (i === periods.length - 1) {
				continousPeriods.push({
					"startTimeInMin": continousPeriodStart,
					"endTimeInMin": currentPeriodEndMinutes
				});
			}
			previousEndMinutes = currentPeriodEndMinutes;
		}

		return continousPeriods;
	};

	var createProbabilityModels = function (scheduleDay, rawProbabilities, dayViewModel, options) {
		if (rawProbabilities == undefined || rawProbabilities.length === 0) {
			return [];
		}

		// If today is full day absence or dayoff, Then hide absence probabilities
		if (options.probabilityType === constants.noneProbabilityType 
			|| (options.probabilityType === constants.absenceProbabilityType
			&& (scheduleDay.IsFullDayAbsence || scheduleDay.IsDayOff))) {
			return [];
		}

		var continousPeriods = [];
		var date = moment(scheduleDay.FixedDate);

		if (options.probabilityType === constants.absenceProbabilityType) {
			continousPeriods = Teleopti.MyTimeWeb.Schedule.Helper.GetContinousPeriods(date, scheduleDay.Periods);
		}

		var boundaries = new Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary(scheduleDay, options.timelines,
			options.probabilityType, rawProbabilities, options.intradayOpenPeriod);

		var probabilityModels = [];

		var filteredRawProbabilities = rawProbabilities;
		if (options.probabilityType == constants.absenceProbabilityType) {
			filteredRawProbabilities = rawProbabilities.filter(function(r) {
				var proStartInMin = moment(r.StartTime).diff(date) / (60 * 1000);
				var proEndInMin = moment(r.EndTime).diff(date) / (60 * 1000);
				var interceptWithSchedulePeriod = false;

				for (i = 0; i < continousPeriods.length; i++) {
					if ((proStartInMin >= continousPeriods[i].startTimeInMin && proStartInMin <= continousPeriods[i].endTimeInMin) || (proEndInMin >= continousPeriods[i].startTimeInMin && proEndInMin <= continousPeriods[i].endTimeInMin)) {
						interceptWithSchedulePeriod = true;
					}
				}
				return interceptWithSchedulePeriod;
			});
		}

		var intervalLenghtInMinutes, tempProbability, probabilityModel;
		for (var i = 0; i < filteredRawProbabilities.length; i++) {
			tempProbability = {
				StartTime: filteredRawProbabilities[i].StartTime,
				Possibility: filteredRawProbabilities[i].Possibility,
				EndTime: filteredRawProbabilities[i].EndTime
			};
			probabilityModel = new Teleopti.MyTimeWeb.Schedule.ProbabilityViewModel(tempProbability, options.probabilityType, boundaries, continousPeriods, options.userTexts, dayViewModel, options.layoutDirection);
			if (!$.isEmptyObject(probabilityModel)) {
				probabilityModels.push(probabilityModel);
			}
		}

		return probabilityModels;
	}

	return {
		GetContinousPeriods: getContinousPeriods,
		CreateProbabilityModels: createProbabilityModels
	};
})(jQuery);