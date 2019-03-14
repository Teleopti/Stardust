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

	function createProbabilityModels(rawProbabilities, dayViewModel, options) {
		if (rawProbabilities === undefined || rawProbabilities.length === 0) {
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
			options.daylightSavingTimeAdjustment,
			options.allowOvernight
		);

		var filteredRawProbabilities = [],
			cellDataList = [];

		if (options.probabilityType === constants.probabilityType.absence) {
			filteredRawProbabilities = filterRawProbabilities(rawProbabilities, mergedPeriods);

			if (filteredRawProbabilities.length === 0) return [];

			var offsetStart = momentUtc(filteredRawProbabilities[0].StartTime).startOf('day');
			filteredRawProbabilities.forEach(function(filteredRawPro) {
				var cellData = createProbabilityCellData(filteredRawPro, offsetStart);
				var trimedCellData = trimIntervalAccordingMergedSchedulePeriod(cellData, mergedPeriods, offsetStart);

				if (trimedCellData.startTimeInMinutes < trimedCellData.endTimeInMinutes)
					cellDataList.push(trimedCellData);
			});
		} else if (options.probabilityType === constants.probabilityType.overtime) {
			filteredRawProbabilities = rawProbabilities;

			if (filteredRawProbabilities.length === 0) return [];

			var offsetStart = momentUtc(filteredRawProbabilities[0].StartTime).startOf('day');
			filteredRawProbabilities.forEach(function(filteredRawPro) {
				var cellData = createProbabilityCellData(filteredRawPro, offsetStart);
				var trimedCellData = trimIntervalAccordingTimeLinePeriodAndBoundaries(
					cellData,
					boundaries,
					offsetStart
				);

				if (trimedCellData.startTimeInMinutes < trimedCellData.endTimeInMinutes)
					cellDataList.push(trimedCellData);
			});
		}

		return buildProbabilityModels(cellDataList, options, dayViewModel, boundaries);
	}

	function buildProbabilityModels(cellDataList, options, dayViewModel, boundaries) {
		if (cellDataList.length === 0) return [];

		var i,
			j,
			probabilityModels = [],
			listLength = cellDataList.length,
			offsetStart = momentUtc(cellDataList[0].startTimeMoment).startOf('day');

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

					//should merge neighboring periods with same probability value for OVERTIME even if probability time is disconnected
					if (options.probabilityType === constants.probabilityType.overtime && !hasSamePossibilityValue) {
						break;
					}

					cellDataList[i].endTimeMoment = cellDataList[j].endTimeMoment;
					cellDataList[i].endTimeInMinutes = cellDataList[j].endTimeInMinutes;
				}
			}

			var probabilityModel = Teleopti.MyTimeWeb.Schedule.ProbabilityViewModel(
				cellDataList[i],
				dayViewModel,
				boundaries,
				options,
				offsetStart
			);

			if (probabilityModel) {
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

	function trimIntervalAccordingMergedSchedulePeriod(probabilityCellData, mergedPeriods, offsetStart) {
		for (var i = 0; i < mergedPeriods.length; i++) {
			var mergedPeriod = mergedPeriods[i];
			if (
				probabilityCellData.startTimeInMinutes <= mergedPeriod.startTimeInMin &&
				probabilityCellData.endTimeInMinutes >= mergedPeriod.startTimeInMin
			) {
				probabilityCellData.startTimeInMinutes = mergedPeriod.startTimeInMin;
				probabilityCellData.startTimeMoment = momentUtc(offsetStart)
					.hours(Math.floor(mergedPeriod.startTimeInMin / 60))
					.minutes(mergedPeriod.startTimeInMin % 60);
			}

			if (
				probabilityCellData.startTimeInMinutes <= mergedPeriod.endTimeInMin &&
				probabilityCellData.endTimeInMinutes >= mergedPeriod.endTimeInMin
			) {
				probabilityCellData.endTimeInMinutes = mergedPeriod.endTimeInMin;
				probabilityCellData.endTimeMoment = momentUtc(offsetStart)
					.hours(Math.floor(mergedPeriod.endTimeInMin / 60))
					.minutes(mergedPeriod.endTimeInMin % 60);
			}
		}

		return probabilityCellData;
	}

	function trimIntervalAccordingTimeLinePeriodAndBoundaries(probabilityCellData, boundaries, offetStart) {
		if (boundaries) {
			if (probabilityCellData.startTimeInMinutes <= boundaries.probabilityStartMinutes) {
				probabilityCellData.startTimeInMinutes = boundaries.probabilityStartMinutes;
				probabilityCellData.startTimeMoment = momentUtc(offetStart)
					.hours(Math.floor(boundaries.probabilityStartMinutes / 60))
					.minutes(boundaries.probabilityStartMinutes % 60);
			}
			if (probabilityCellData.endTimeInMinutes >= boundaries.probabilityEndMinutes) {
				probabilityCellData.endTimeInMinutes = boundaries.probabilityEndMinutes;
				probabilityCellData.endTimeMoment = momentUtc(offetStart)
					.hours(Math.floor(boundaries.probabilityEndMinutes / 60))
					.minutes(boundaries.probabilityEndMinutes % 60);
			}
		}

		return probabilityCellData;
	}

	function filterRawProbabilities(rawProbabilities, continousPeriods) {
		var offsetStart = momentUtc(rawProbabilities[0].StartTime).startOf('day');

		var result = rawProbabilities.filter(function(r) {
			var probabilityStartInMin = getTimeOffsetInMinute(offsetStart, r.StartTime);
			var probabilityEndInMin = getTimeOffsetInMinute(offsetStart, r.EndTime);

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

	function createProbabilityCellData(rawProbability, offsetStart) {
		return {
			startTimeMoment: momentUtc(rawProbability.StartTime),
			endTimeMoment: momentUtc(rawProbability.EndTime),
			startTimeInMinutes: getTimeOffsetInMinute(offsetStart, rawProbability.StartTime),
			endTimeInMinutes: getTimeOffsetInMinute(offsetStart, rawProbability.EndTime),
			possibility: rawProbability.Possibility
		};
	}

	function getTimeOffsetInMinute(offsetStart, time) {
		return momentUtc(time).diff(offsetStart) / (60 * 1000);
	}

	return {
		GetMergedPeriods: getMergedPeriods,
		CreateProbabilityModels: createProbabilityModels
	};
})(jQuery);
