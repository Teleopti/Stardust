Teleopti.MyTimeWeb.Schedule.ProbabilityModels = (function($) {
	var constants = Teleopti.MyTimeWeb.Common.Constants;
	var momentUtc = Teleopti.MyTimeWeb.Common.MomentAsUTCIgnoringTimezone;

	function getMergedPeriods(baseDate, periods) {
		if (!periods || periods.length === 0) return [];

		var mergedPeriods = [];
		baseDate = momentUtc(baseDate.format('YYYY-MM-DDTHH:mm:ss'));

		periods.forEach(function(period) {
			var start = momentUtc(period.StartTime).diff(baseDate) / (60 * 1000);
			var end = momentUtc(period.EndTime).diff(baseDate) / (60 * 1000);

			if (start === end) return;

			if (start < 0) start = 0;

			if (
				mergedPeriods[mergedPeriods.length - 1] &&
				start === mergedPeriods[mergedPeriods.length - 1].endTimeInMin
			) {
				mergedPeriods[mergedPeriods.length - 1].endTimeInMin = end;
			} else {
				mergedPeriods.push({
					startTimeInMin: start,
					endTimeInMin: end
				});
			}
		});

		return mergedPeriods;
	}

	function getIntervalStartMinutes(startTime) {
		var startMoment = momentUtc(startTime);
		var startMinutes = startMoment.diff(momentUtc(startTime).startOf('day'));

		return (startMinutes > 0 ? startMinutes : 0) / (60 * 1000);
	}

	function getIntervalEndMinutes(startTime, endTime) {
		var startMoment = momentUtc(startTime);
		var endMoment = momentUtc(endTime);

		return endMoment.isSame(startMoment, 'day')
			? endMoment.diff(startMoment.startOf('day')) / (60 * 1000)
			: constants.totalMinutesOfOneDay - 1;
	}

	function createProbabilityCellData(rawProbability) {
		var intervalStartMinutes = getIntervalStartMinutes(rawProbability.StartTime);
		var intervalEndMinutes = getIntervalEndMinutes(rawProbability.StartTime, rawProbability.EndTime);

		return {
			startTimeMoment: momentUtc(rawProbability.StartTime),
			endTimeMoment: momentUtc(rawProbability.EndTime),
			startTimeInMinutes: intervalStartMinutes,
			endTimeInMinutes: intervalEndMinutes,
			possibility: rawProbability.Possibility
		};
	}

	function createProbabilityModels(rawProbabilities, dayViewModel, options) {
		if (rawProbabilities == undefined || rawProbabilities.length === 0) {
			return [];
		}

		if (options.probabilityType === constants.probabilityType.none) return [];

		if (options.probabilityType === constants.probabilityType.absence) {
			if (dayViewModel.isFullDayAbsence) return [];
			if (dayViewModel.isDayOff && !existsScheduleFromYesterday(dayViewModel)) return [];
		}

		var mergedPeriods = [];
		var date = moment(dayViewModel.fixedDate);

		if (options.probabilityType === constants.probabilityType.absence) {
			mergedPeriods = Teleopti.MyTimeWeb.Schedule.ProbabilityModels.GetMergedPeriods(date, dayViewModel.periods);
		}

		var boundaries = Teleopti.MyTimeWeb.Schedule.ProbabilityBoundary(
			dayViewModel,
			options.timelines,
			options.probabilityType,
			rawProbabilities,
			options.daylightSavingTimeAdjustment
		);

		var probabilityModels = [],
			filteredRawProbabilities = [],
			cellDataList = [];

		if (options.probabilityType === constants.probabilityType.absence) {
			filteredRawProbabilities = filterRawProbabilities(rawProbabilities, mergedPeriods);
			filteredRawProbabilities.forEach(function(filteredRawPro) {
				var cellData = createProbabilityCellData(filteredRawPro);
				var trimedCellData = trimIntervalAccordingMergedSchedulePeriod(cellData, mergedPeriods);

				if (trimedCellData.startTimeInMinutes < trimedCellData.endTimeInMinutes)
					cellDataList.push(trimedCellData);
			});
		} else if (options.probabilityType === constants.probabilityType.overtime) {
			filteredRawProbabilities = rawProbabilities;
			filteredRawProbabilities.forEach(function(filteredRawPro) {
				var cellData = createProbabilityCellData(filteredRawPro);
				var trimedCellData = trimIntervalAccordingTimeLinePeriodAndBoundaries(cellData, boundaries);

				if (trimedCellData.startTimeInMinutes < trimedCellData.endTimeInMinutes)
					cellDataList.push(trimedCellData);
			});
		}

		var i,
			j,
			probabilityModel,
			listLength = cellDataList.length;
		for (i = 0; i < listLength; i = j) {
			j = i + 1;
			if (options.mergeSameIntervals) {
				for (; j < listLength; j++) {
					var hasSamePossibilityValue = cellDataList[j].possibility === cellDataList[i].possibility;
					var isConnectedPossibility =
						cellDataList[i].endTimeInMinutes === cellDataList[j].startTimeInMinutes;

					if (
						options.probabilityType === constants.probabilityType.absence &&
						(!hasSamePossibilityValue || !isConnectedPossibility)
					) {
						break;
					}

					if (options.probabilityType === constants.probabilityType.overtime && !hasSamePossibilityValue) {
						break;
					}

					cellDataList[i].endTimeMoment = cellDataList[j].endTimeMoment;
					cellDataList[i].endTimeInMinutes = cellDataList[j].endTimeInMinutes;
				}
			}

			probabilityModel = Teleopti.MyTimeWeb.Schedule.ProbabilityViewModel(
				cellDataList[i],
				dayViewModel,
				boundaries,
				options
			);

			if (!$.isEmptyObject(probabilityModel)) {
				probabilityModels.push(probabilityModel);
			}
		}

		return probabilityModels;
	}

	function existsScheduleFromYesterday(dayViewModel) {
		if (dayViewModel.periods == undefined || dayViewModel.periods.length === 0) return false;
		var period = dayViewModel.periods[0];
		var startDate = moment(period.StartTime).format(constants.serviceDateTimeFormat.dateOnly);
		var endDate = moment(period.EndTime).format(constants.serviceDateTimeFormat.dateOnly);
		return moment(startDate).isBefore(endDate);
	}

	function trimIntervalAccordingMergedSchedulePeriod(probabilityCellData, mergedPeriods) {
		for (var i = 0; i < mergedPeriods.length; i++) {
			var mergedPeriod = mergedPeriods[i];
			if (
				probabilityCellData.startTimeInMinutes <= mergedPeriod.startTimeInMin &&
				probabilityCellData.endTimeInMinutes >= mergedPeriod.startTimeInMin
			) {
				probabilityCellData.startTimeInMinutes = mergedPeriod.startTimeInMin;
				probabilityCellData.startTimeMoment
					.hours(Math.floor(mergedPeriod.startTimeInMin / 60))
					.minutes(mergedPeriod.startTimeInMin % 60);
			}

			if (
				probabilityCellData.startTimeInMinutes <= mergedPeriod.endTimeInMin &&
				probabilityCellData.endTimeInMinutes >= mergedPeriod.endTimeInMin
			) {
				probabilityCellData.endTimeInMinutes = mergedPeriod.endTimeInMin;
				probabilityCellData.endTimeMoment
					.hours(Math.floor(mergedPeriod.endTimeInMin / 60))
					.minutes(mergedPeriod.endTimeInMin % 60);
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
		var result = rawProbabilities.filter(function(r) {
			var probabilityStartInMin = getIntervalStartMinutes(r.StartTime);
			var probabilityEndInMin = getIntervalEndMinutes(r.StartTime, r.EndTime);

			var interceptWithSchedulePeriod = false;
			for (var i = 0; i < continousPeriods.length; i++) {
				if (
					(probabilityStartInMin >= continousPeriods[i].startTimeInMin &&
						probabilityStartInMin <= continousPeriods[i].endTimeInMin) ||
					(probabilityEndInMin >= continousPeriods[i].startTimeInMin &&
						probabilityEndInMin <= continousPeriods[i].endTimeInMin)
				) {
					interceptWithSchedulePeriod = true;
				}
			}

			return interceptWithSchedulePeriod;
		});

		return result;
	}

	return {
		GetMergedPeriods: getMergedPeriods,
		CreateProbabilityModels: createProbabilityModels
	};
})(jQuery);
