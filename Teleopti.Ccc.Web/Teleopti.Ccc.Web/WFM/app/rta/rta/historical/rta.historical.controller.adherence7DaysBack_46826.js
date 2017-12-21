(function () {
	'use strict';
	angular.module('wfm.rta').controller('RtaHistoricalController46826', RtaHistoricalController);
	RtaHistoricalController.$inject = ['$http', '$state', '$stateParams', 'rtaService', '$translate', 'Toggle'];

	function RtaHistoricalController($http, $state, $stateParams, rtaService, $translate, toggles) {
		var vm = this;

		vm.highlighted = {};
		vm.diamonds = [];
		vm.cards = [];

		$stateParams.open = ($stateParams.open || "false");

		var nextDayInParams = moment($stateParams.date).add(1, 'day').format('YYYYMMDD');
		var previousDayInParams = moment($stateParams.date).subtract(1, 'day').format('YYYYMMDD');
		
		vm.ooaTooltipTime = function (time) {
			if (time == null)
				return '';
			return time.format('HH:mm:ss');
		};

		var shiftInfo;

		function positionForTime(offsetTime, time, totalSeconds) {
			var diff = moment(time).diff(moment(offsetTime), 'seconds');
			return (diff / totalSeconds) * 100 + '%';
		}

		function makeOffsetCalculator(offsetTime, totalSeconds) {
			return function (time) {
				return positionForTime(offsetTime, time, totalSeconds);
			}
		}

		function makeWidthCalculator(totalSeconds) {
			return function (offsetTime, time) {
				return positionForTime(offsetTime, time, totalSeconds);
			}
		}

		var calculateWidth;
		var calculateOffset;

		$http.get('../api/HistoricalAdherence/ForPerson', {
			params: {
				personId: $stateParams.personId,
				date: $stateParams.date
			}
		}).then(function (response) {
			var data = response.data;

			data.Schedules = data.Schedules || [];
			data.OutOfAdherences = data.OutOfAdherences || [];
			data.Changes = data.Changes || [];
			data.Timeline = data.Timeline || {};
			data.Interval = data.Interval || {};
			
			shiftInfo = buildShiftInfo(data);

			calculateWidth = makeWidthCalculator(shiftInfo.timeWindowSeconds);
			calculateOffset = makeOffsetCalculator(shiftInfo.timeWindowStart, shiftInfo.timeWindowSeconds);

			vm.personId = data.PersonId;
			vm.agentName = data.AgentName;
			// remove the use of now when removing RTA_ViewHistoricalAhderenceForRecentShifts_46786
			vm.date = moment($stateParams.date || data.Now).format('YYYY-MM-DD');
			vm.adherencePercentage = data.AdherencePercentage;
			vm.showAdherencePercentage = data.AdherencePercentage != null;

			vm.currentTimeOffset = calculateOffset(data.Now);

			vm.agentsFullSchedule = buildAgentsFullSchedule(data.Schedules);

			vm.outOfAdherences = buildAgentOutOfAdherences(data);

			vm.fullTimeline = buildTimeline(data.Timeline);

			vm.cards = mapChanges(data.Changes, data.Schedules);

			vm.diamonds = buildDiamonds(data);
	
			vm.disabledNext = $stateParams.date >= data.Interval.EndDate;
			vm.disabledPrev = $stateParams.date <= data.Interval.StartDate;
			vm.nextDay = moment($stateParams.date).add(1, 'day').format('YYYY-MM-DD');
			vm.previousDay = moment($stateParams.date).subtract(1, 'day').format('YYYY-MM-DD');
		});
		
		vm.goToNext = function (){
			$state.go($state.current.name, {date: nextDayInParams,  personId: $stateParams.personId});
		};

		vm.goToPrevious = function (){
			$state.go($state.current.name, {date: previousDayInParams,  personId: $stateParams.personId});
		};

		
		function buildAgentOutOfAdherences(data) {
			return data.OutOfAdherences.map(function (ooa) {
				var startTime = moment(ooa.StartTime);
				var startTimeFormatted = shiftInfo.timeWindowStart > startTime ?
					startTime.format('YYYY-MM-DD HH:mm:ss') :
					startTime.format('HH:mm:ss');
				var endTime = ooa.EndTime != null ? ooa.EndTime : data.Now;
				var endTimeFormatted = ooa.EndTime != null ? moment(ooa.EndTime).format('HH:mm:ss') : '';
				return {
					Width: calculateWidth(startTime, endTime),
					Offset: calculateOffset(startTime),
					StartTime: startTimeFormatted,
					EndTime: endTimeFormatted
				};
			});
		}

		function buildAgentsFullSchedule(schedules) {
			return schedules.map(function (layer) {
				return {
					Width: calculateWidth(layer.StartTime, layer.EndTime),
					Offset: calculateOffset(layer.StartTime),
					StartTime: moment(layer.StartTime),
					EndTime: moment(layer.EndTime),
					Color: layer.Color
				};
			});
		}

		function buildShiftInfo(data) {
			var start = moment(data.Timeline.StartTime);
			var end = moment(data.Timeline.EndTime);
			var shiftInfo = {
				totalSeconds: end.diff(start, 'seconds')
			};

			var schedule = data.Schedules;
			if (schedule.length > 0) {
				var shiftStartTime = moment(schedule[0].StartTime);
				var shiftEndTime = moment(schedule[schedule.length - 1].EndTime);
				start = shiftStartTime.clone();
				start.startOf('minute');
				end = shiftEndTime.clone();
				end.endOf('minute');

				shiftInfo.totalSeconds = shiftEndTime.diff(shiftStartTime, 'seconds');
			}

			shiftInfo.timeWindowSeconds = shiftInfo.totalSeconds % 3600 == 0 ? shiftInfo.totalSeconds + 7200 : shiftInfo.totalSeconds + (3600 - (shiftInfo.totalSeconds % 3600)) + 7200;
			shiftInfo.timeWindowStart = start.clone().add(-1, 'hour');

			return shiftInfo;
		}

		function buildDiamonds(data) {
			return data.Changes.map(function (change, i) {
				change.Offset = calculateOffset(change.Time);
				change.RuleColor = !change.RuleColor ? "rgba(0,0,0,0.54)" : change.RuleColor;
				change.Color = change.RuleColor;
				change.click = function () {
					vm.diamonds.forEach(function (d) {
						d.highlight = false;
					});
					change.highlight = true;
					vm.cards.find(function (card) {
						return card.Items.includes(change);
					}).isOpen = true;
				};
				return change;
			});
		}

		function mapChanges(changes, schedules) {

			var makeCard = function (header, color, startTime, endTime) {
				return {
					Header: header,
					Items: [],
					Color: color,
					isOpen: $stateParams.open != "false",
					StartTime: startTime,
					EndTime: endTime
				}
			};

			if (schedules.length === 0) {
				var card = makeCard($translate.instant('NoShift'), 'black');
				card.Items = changes;
				return [card];
			}

			var beforeShift = [makeCard($translate.instant('BeforeShiftStart'), 'black', '0000-01-01T00:00:00', schedules[0].StartTime)];
			var afterShift = [makeCard($translate.instant('AfterShiftEnd'), 'black', schedules[schedules.length - 1].EndTime, '9999-01-01T00:00:00')];
			return beforeShift
				.concat(schedules.map(function (l) {
					var activityStart = moment(l.StartTime);
					var activityEnd = moment(l.EndTime);
					var header = l.Name + ' ' + activityStart.format('HH:mm') + ' - ' + activityEnd.format('HH:mm');
					return makeCard(header, l.Color, l.StartTime, l.EndTime);
				}))
				.concat(afterShift)
				.map(function (card) {
					card.Items = changes.filter(function (change) {
						return change.Time >= card.StartTime && change.Time < card.EndTime;
					});
					return card
				})
				.filter(function (card) {
					return card.Items.length > 0;
				});

		}

		function buildTimeline(times) {
			var timeline = [];
			var currentMoment = moment(times.StartTime);
			var endTime = moment(times.EndTime);
			var totalHours = (endTime.diff(currentMoment, 'minutes') % 60 == 0) ? endTime.diff(currentMoment, 'hours') : endTime.diff(currentMoment, 'hours') + 1;
			var hourPercent = 100 / totalHours;

			for (var i = 1; i < totalHours; i++) {
				currentMoment.add(1, 'hour');
				var percent = hourPercent * i;
				timeline.push({
					Offset: percent + '%',
					Time: currentMoment.clone()
				});
			}

			return timeline;
		}

	}
})();
