﻿Teleopti.MyTimeWeb.Schedule.ProbabilityModels = (function ($) {
	var constants = Teleopti.MyTimeWeb.Common.Constants;

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
			possibility: rawProbability.Possibility
		};
	}

	var createProbabilityModels = function (rawProbabilities, dayViewModel, options) {
		if (rawProbabilities == undefined || rawProbabilities.length === 0) {
			return [];
		}

		if (options.probabilityType === constants.probabilityType.none)
			return [];

		if (options.probabilityType === constants.probabilityType.absence) {
			if (dayViewModel.isFullDayAbsence)
				return [];
			if (dayViewModel.isDayOff && !existsScheduleFromYesterday(dayViewModel))
				return [];
		}

		var continousPeriods = [];
		var date = moment(dayViewModel.fixedDate());

		if (options.probabilityType === constants.probabilityType.absence) {
			continousPeriods = Teleopti.MyTimeWeb.Schedule.ProbabilityModels.GetContinousPeriods(date, dayViewModel.periods);
		}

		var boundaries = new Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary(dayViewModel, options.timelines,
			options.probabilityType, rawProbabilities, options.intradayOpenPeriod);

		var probabilityModels = [], filteredRawProbabilities = [], cellDataList = [];

		if (options.probabilityType === constants.probabilityType.absence) {
			filteredRawProbabilities = filterRawProbabilities(rawProbabilities, continousPeriods);
			filteredRawProbabilities.forEach(function (filteredRawPro) {
				var cellData = createProbabilityCellData(filteredRawPro);
				var trimedCellData = trimIntervalAccordingContinuousSchedulePeriod(cellData, continousPeriods);

				if (trimedCellData.startTimeInMinutes < trimedCellData.endTimeInMinutes)
					cellDataList.push(trimedCellData);
			});
		} else if (options.probabilityType === constants.probabilityType.overtime) {
			filteredRawProbabilities = rawProbabilities;
			filteredRawProbabilities.forEach(function (filteredRawPro) {
				var cellData = createProbabilityCellData(filteredRawPro);
				var trimedCellData = trimIntervalAccordingTimeLinePeriodAndBoundaries(cellData, boundaries);

				if (trimedCellData.startTimeInMinutes < trimedCellData.endTimeInMinutes)
					cellDataList.push(trimedCellData);
			});
		}

		var i, j, probabilityModel, listLength = cellDataList.length;
		for (i = 0; i < listLength; i = j) {
			j = i + 1;
			if (options.mergeSameIntervals) {
				for (; j < listLength; j++) {
					var hasSamePossibilityValue = cellDataList[j].possibility === cellDataList[i].possibility;
					var isConnectedPossibility = cellDataList[i].endTimeInMinutes === cellDataList[j].startTimeInMinutes;
					if (!hasSamePossibilityValue || !isConnectedPossibility) {
						break;
					}

					cellDataList[i].endTimeMoment = cellDataList[j].endTimeMoment;
					cellDataList[i].endTimeInMinutes = cellDataList[j].endTimeInMinutes;
				}
			}

			probabilityModel = new Teleopti.MyTimeWeb.Schedule.ProbabilityViewModel(cellDataList[i], options.probabilityType,
				boundaries, options.userTexts, dayViewModel, options.layoutDirection, options.hideProbabilityEarlierThanNow);

			if (!$.isEmptyObject(probabilityModel)) {
				probabilityModels.push(probabilityModel);
			}
		}

		probabilityModels.forEach(function (p, i, arr) {

			if (arr[i + 1]) {
				var isContinuedPeriod = arr[i + 1].startTimeInMinutes === p.endTimeInMinutes;
				if (isContinuedPeriod) {
					p.intervalLength = (arr[i + 1].startPosition - p.startPosition).toFixed(2);

					if (p.styleJson['width'] && p.styleJson['width'].length > 0) {
						p.styleJson['width'] = p.intervalLength + "%";
						return;
					}

					if (p.styleJson['height'] && p.styleJson['height'].length > 0) {
						p.styleJson['height'] = p.intervalLength + "%";
					}
				}
			}
		});

		return probabilityModels;
	};

	function existsScheduleFromYesterday(dayViewModel) {
		if (dayViewModel.periods == undefined || dayViewModel.periods.length === 0)
			return false;
		var period = dayViewModel.periods[0];
		var startDate = moment(period.StartTime).format(constants.dateOnlyFormat);
		var endDate = moment(period.EndTime).format(constants.dateOnlyFormat);
		return moment(startDate).isBefore(endDate);
	}

	function trimIntervalAccordingContinuousSchedulePeriod(probabilityCellData, continousPeriods) {
		for (var i = 0; i < continousPeriods.length; i++) {
			var continousPeriod = continousPeriods[i];
			if ((probabilityCellData.startTimeInMinutes <= continousPeriod.startTimeInMin &&
				probabilityCellData.endTimeInMinutes >= continousPeriod.startTimeInMin)) {
				probabilityCellData.startTimeInMinutes = continousPeriod.startTimeInMin;
				probabilityCellData.startTimeMoment
					.hours(Math.floor(continousPeriod.startTimeInMin / 60))
					.minutes(continousPeriod.startTimeInMin % 60);
			}

			if ((probabilityCellData.startTimeInMinutes <= continousPeriod.endTimeInMin &&
				probabilityCellData.endTimeInMinutes >= continousPeriod.endTimeInMin)) {
				probabilityCellData.endTimeInMinutes = continousPeriod.endTimeInMin;
				probabilityCellData.endTimeMoment
					.hours(Math.floor(continousPeriod.endTimeInMin / 60))
					.minutes(continousPeriod.endTimeInMin % 60);
			}
		}

		return probabilityCellData;
	}

	function trimIntervalAccordingTimeLinePeriodAndBoundaries(probabilityCellData, boundaries) {
		if (boundaries) {
			if (probabilityCellData.startTimeInMinutes <= boundaries.probabilityStartMinutes) {
				probabilityCellData.startTimeInMinutes = boundaries.probabilityStartMinutes;
				probabilityCellData.startTimeMoment
					.hours(Math.floor(boundaries.probabilityStartMinutes / 60))
					.minutes(boundaries.probabilityStartMinutes % 60);
			}
			if (probabilityCellData.endTimeInMinutes >= boundaries.probabilityEndMinutes) {
				probabilityCellData.endTimeInMinutes = boundaries.probabilityEndMinutes;
				probabilityCellData.endTimeMoment
					.hours(Math.floor(boundaries.probabilityEndMinutes / 60))
					.minutes(boundaries.probabilityEndMinutes % 60);
			}
		}

		return probabilityCellData;
	}

	function filterRawProbabilities(rawProbabilities, continousPeriods) {
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