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

	function getIntervalStartMinutes(startTime) {
		var startMoment = moment(startTime);
		var startMinutes = startMoment.diff(moment(startTime).startOf("day"));

		return (startMinutes > 0 ? startMinutes : 0) / (60 * 1000);
	}

	function getIntervalEndMinutes(startTime, endTime) {
		var startMoment = moment(startTime);
		var endMoment = moment(endTime);

		return endMoment.isSame(startMoment, "day")
		? endMoment.diff(startMoment.startOf("day")) / (60 * 1000)
		: constants.totalMinutesOfOneDay - 1;
	}

	var createProbabilityCellData = function (rawProbability) {
		var intervalStartMinutes = getIntervalStartMinutes(rawProbability.StartTime);
		var intervalEndMinutes = getIntervalEndMinutes(rawProbability.StartTime, rawProbability.EndTime);

		return {
			startTimeMoment: moment(rawProbability.StartTime),
			endTimeMoment: moment(rawProbability.EndTime),
			startTimeInMinutes: intervalStartMinutes,
			endTimeInMinutes: intervalEndMinutes,
			possibility: rawProbability.Possibility,
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

		var probabilityModels = [], filteredRawProbabilities = [], filteredRawProbabilityCellDataList = [];

		if(options.probabilityType == constants.absenceProbabilityType){
			filteredRawProbabilities = filterRawProbabilities(rawProbabilities, continousPeriods);
			filteredRawProbabilities.forEach(function(filteredRawPro){
				var cellData = createProbabilityCellData(filteredRawPro);
				var trimedCellData = trimIntervalAccordingContinuousSchedulePeriod(cellData, continousPeriods);

				if(trimedCellData.startTimeInMinutes < trimedCellData.endTimeInMinutes )
					filteredRawProbabilityCellDataList.push(trimedCellData);
			});
		}else if(options.probabilityType == constants.overtimeProbabilityType){
			filteredRawProbabilities = rawProbabilities;
			filteredRawProbabilities.forEach(function(filteredRawPro){
				var cellData = createProbabilityCellData(filteredRawPro);
				var trimedCellData = trimIntervalAccordingTimeLinePeriod(cellData, boundaries);

				if(trimedCellData.startTimeInMinutes < trimedCellData.endTimeInMinutes )
					filteredRawProbabilityCellDataList.push(trimedCellData);
			});
		}

		var i, j, probabilityModel, listLength = filteredRawProbabilityCellDataList.length;

		for (i = 0; i < listLength; i = j) {
			j = i + 1;
			if (options.mergeSameIntervals) {
				for (; j < listLength; j++) {
					var hasSamePossibilityValue = filteredRawProbabilityCellDataList[j].possibility == filteredRawProbabilityCellDataList[i].possibility;

					var isConnectedPossibility = filteredRawProbabilityCellDataList[i].endTimeInMinutes == filteredRawProbabilityCellDataList[j].startTimeInMinutes;

					if (!hasSamePossibilityValue || !isConnectedPossibility) {
						break;
					}

					filteredRawProbabilityCellDataList[i].endTimeMoment = filteredRawProbabilityCellDataList[j].endTimeMoment;
					filteredRawProbabilityCellDataList[i].endTimeInMinutes = filteredRawProbabilityCellDataList[j].endTimeInMinutes;
				}
			}

			probabilityModel = new Teleopti.MyTimeWeb.Schedule.ProbabilityViewModel(filteredRawProbabilityCellDataList[i], options.probabilityType, boundaries, options.userTexts, dayViewModel, options.layoutDirection, options.hideProbabilityEarlierThanNow);

			if (!$.isEmptyObject(probabilityModel)) {
				probabilityModels.push(probabilityModel);
			}
		}

		return probabilityModels;
	};

	function trimIntervalAccordingContinuousSchedulePeriod(probabilityCellData, continousPeriods) {
		for (var i = 0; i < continousPeriods.length; i++) {
			var continousPeriod = continousPeriods[i];
			if ((probabilityCellData.startTimeInMinutes <= continousPeriod.startTimeInMin && probabilityCellData.endTimeInMinutes >= continousPeriod.startTimeInMin)) {
				probabilityCellData.startTimeInMinutes = continousPeriod.startTimeInMin;
				probabilityCellData.startTimeMoment.hours(Math.floor(continousPeriod.startTimeInMin / 60)).minutes(continousPeriod.startTimeInMin % 60);
			}

			if ((probabilityCellData.startTimeInMinutes <= continousPeriod.endTimeInMin && probabilityCellData.endTimeInMinutes >= continousPeriod.endTimeInMin)) {
				probabilityCellData.endTimeInMinutes = continousPeriod.endTimeInMin;
				probabilityCellData.endTimeMoment.hours(Math.floor(continousPeriod.endTimeInMin / 60)).minutes(continousPeriod.endTimeInMin % 60);
			}
		}

		return probabilityCellData;
	}

	function trimIntervalAccordingTimeLinePeriod(probabilityCellData, boundaries) {
		if(boundaries){
			if(probabilityCellData.startTimeInMinutes <= boundaries.timelineStartMinutes){
				probabilityCellData.startTimeInMinutes = boundaries.timelineStartMinutes;
				probabilityCellData.startTimeMoment.hours(Math.floor(boundaries.timelineStartMinutes / 60)).minutes(boundaries.timelineStartMinutes % 60);
			}
			if(probabilityCellData.endTimeInMinutes >= boundaries.timelineEndMinutes){
				probabilityCellData.endTimeInMinutes = boundaries.timelineEndMinutes;
				probabilityCellData.endTimeMoment.hours(Math.floor(boundaries.timelineEndMinutes / 60)).minutes(boundaries.timelineEndMinutes % 60);
			}
		}

		return probabilityCellData;
	}

	function filterRawProbabilities(rawProbabilities, continousPeriods){
		var result = rawProbabilities.filter(function (r) {
			var probabilityStartInMin = getIntervalStartMinutes(r.StartTime);
			var probabilityEndInMin = getIntervalEndMinutes(r.StartTime, r.EndTime);

			var interceptWithSchedulePeriod = false;
			for (var i = 0; i < continousPeriods.length; i++) {
				if ((probabilityStartInMin >= continousPeriods[i].startTimeInMin && probabilityStartInMin <= continousPeriods[i].endTimeInMin) ||
					(probabilityEndInMin >= continousPeriods[i].startTimeInMin && probabilityEndInMin <= continousPeriods[i].endTimeInMin)) {
					interceptWithSchedulePeriod = true;
				}
			}

			return interceptWithSchedulePeriod;
		});

		return result;
	}

	return {
		GetContinousPeriods: getContinousPeriods,
		CreateProbabilityModels: createProbabilityModels
	};
})(jQuery);