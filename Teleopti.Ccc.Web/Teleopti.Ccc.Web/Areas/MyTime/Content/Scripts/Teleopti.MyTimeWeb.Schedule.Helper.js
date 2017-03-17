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

	function getIntervalStartMinutes(startOfToday, startMoment) {
		var startMinutes = startMoment.diff(startOfToday);
		return (startMinutes > 0 ? startMinutes : 0) / (60 * 1000);
	}

	function getIntervalEndMinutes(startOfToday, startMoment, endMoment) {
		return endMoment.isSame(startMoment, "day")
		? endMoment.diff(startOfToday) / (60 * 1000)
		: constants.totalMinutesOfOneDay - 1;
	}

	var createMiddleRawProbability = function (boundaries, continousPeriods, probabilityType, rawProbability) {
		var startOfToday = moment(rawProbability.StartTime).startOf("day");
		var startMoment = moment(rawProbability.StartTime);
		var endMoment = moment(rawProbability.EndTime);
		var intervalEndMinutes = getIntervalEndMinutes(startOfToday, startMoment, endMoment);
		var intervalStartMinutes = getIntervalStartMinutes(startOfToday, startMoment);

		var visible = false;
		if (probabilityType === constants.absenceProbabilityType) {
			for (var i = 0; i < continousPeriods.length; i++) {
				var continousPeriod = continousPeriods[i];
				if (continousPeriod.startTimeInMin <= intervalStartMinutes && intervalEndMinutes <= continousPeriod.endTimeInMin) {
					visible = true;
					break;
				}
			}
		} else if (probabilityType === constants.overtimeProbabilityType) {
			visible = boundaries.probabilityStartMinutes <= intervalStartMinutes &&
				intervalEndMinutes <= boundaries.probabilityEndMinutes;
		}

		var shouldGenerate = boundaries.probabilityStartMinutes <= intervalStartMinutes &&
			intervalEndMinutes <= boundaries.probabilityEndMinutes;
		return {
			startTime: startMoment,
			endTime: endMoment,
			startTimeInMinutes: intervalStartMinutes,
			endTimeInMinutes: intervalEndMinutes,
			possibility: rawProbability.Possibility,
			shouldGenerateViewModel: shouldGenerate && visible
		};
	}

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

		var i = 0;
		var filteredRawProbabilities = options.probabilityType === constants.absenceProbabilityType
			? rawProbabilities.filter(function (r) {
				var proStartInMin = moment(r.StartTime).diff(date) / (60 * 1000);
				var proEndInMin = moment(r.EndTime).diff(date) / (60 * 1000);
				var interceptWithSchedulePeriod = false;

				for (i = 0; i < continousPeriods.length; i++) {
					if ((proStartInMin >= continousPeriods[i].startTimeInMin && proStartInMin <= continousPeriods[i].endTimeInMin) ||
						(proEndInMin >= continousPeriods[i].startTimeInMin && proEndInMin <= continousPeriods[i].endTimeInMin)) {
						interceptWithSchedulePeriod = true;
					}
				}
				return interceptWithSchedulePeriod;
			})
			: rawProbabilities;

		i = 0;
		var middleRawProbability = undefined;
		while (i < filteredRawProbabilities.length) {
			if (middleRawProbability == undefined) {
				middleRawProbability = createMiddleRawProbability(boundaries, continousPeriods,
					options.probabilityType, filteredRawProbabilities[i]);
			}

			var j = i + 1;
			if (options.mergeSameIntervals) {
				while (j < filteredRawProbabilities.length) {
					var newMiddleRawProbability = createMiddleRawProbability(boundaries, continousPeriods,
						options.probabilityType, filteredRawProbabilities[j]);
					if (newMiddleRawProbability.shouldGenerateViewModel !== middleRawProbability.shouldGenerateViewModel ||
						newMiddleRawProbability.possibility !== middleRawProbability.possibility) {
						break;
					}

					middleRawProbability.endTime = newMiddleRawProbability.endTime;
					middleRawProbability.endTimeInMinutes = newMiddleRawProbability.endTimeInMinutes;
					j++;
				}
			}

			if (middleRawProbability.shouldGenerateViewModel) {
				var probabilityModel = new Teleopti.MyTimeWeb.Schedule.ProbabilityViewModel(middleRawProbability,
					options.probabilityType, boundaries, continousPeriods,
					options.userTexts, dayViewModel, options.layoutDirection);

				if (!$.isEmptyObject(probabilityModel)) {
					probabilityModels.push(probabilityModel);
				}
			}

			middleRawProbability = undefined;
			i = j;
		}

		return probabilityModels;
	}

	return {
		GetContinousPeriods: getContinousPeriods,
		CreateProbabilityModels: createProbabilityModels
	};
})(jQuery);