(function () {
	'use strict';
	angular.module('wfm.rta').controller('RtaHistoricalController', RtaHistoricalController);
	RtaHistoricalController.$inject = ['$stateParams', 'rtaService', '$translate', 'Toggle'];

	function RtaHistoricalController($stateParams, rtaService, $translate, toggles) {
		var vm = this;

		var id = $stateParams.personId;
		vm.highlighted = {};
		vm.diamonds = [];
		vm.cards = [];
		$stateParams.open = ($stateParams.open || "false")

		vm.ooaTooltipTime = function (time) {
			if (time == null)
				return '';
			return time.format('HH:mm:ss');
		};
		
		var shiftInfo;
		
		rtaService.getAgentHistoricalData(id)
			.then(function (data) {

				data.Schedules = data.Schedules || [];
				data.OutOfAdherences = data.OutOfAdherences || [];
				data.Changes = data.Changes || [];

				shiftInfo = buildShiftInfo(data);

				vm.personId = data.PersonId;
				vm.agentName = data.AgentName;
				vm.date = moment(data.Now);

				vm.currentTimeOffset = calculateWidth(shiftInfo.timeWindowStart, data.Now, shiftInfo.timeWindowSeconds);

				vm.agentsFullSchedule = buildAgentsFullSchedule(data.Schedules);

				vm.outOfAdherences = buildAgentOutOfAdherences(data);

				vm.fullTimeline = buildTimeline(data.Timeline);

				vm.cards = mapChanges(data.Changes, data.Schedules);

				vm.diamonds = buildDiamonds(data);

			});

		function buildAgentOutOfAdherences(data) {
			return data.OutOfAdherences.map(function (ooa) {
				var startTime = moment(ooa.StartTime);
				var startTimeFormatted = shiftInfo.timeWindowStart > startTime ?
					startTime.format('YYYY-MM-DD HH:mm:ss') :
					startTime.format('HH:mm:ss');
				var endTime = ooa.EndTime != null ? ooa.EndTime : data.Now;
				var endTimeFormatted = ooa.EndTime != null ? moment(ooa.EndTime).format('HH:mm:ss') : '';
				return {
					Width: calculateWidth(startTime, endTime, shiftInfo.timeWindowSeconds),
					Offset: calculateWidth(shiftInfo.timeWindowStart, startTime, shiftInfo.timeWindowSeconds),
					StartTime: startTimeFormatted,
					EndTime: endTimeFormatted
				};
			});
		}

		function buildAgentsFullSchedule(schedules) {
			return schedules.map(function (layer) {
				return {
					Width: calculateWidth(layer.StartTime, layer.EndTime, shiftInfo.timeWindowSeconds),
					Offset: calculateWidth(shiftInfo.timeWindowStart, layer.StartTime, shiftInfo.timeWindowSeconds),
					StartTime: moment(layer.StartTime),
					EndTime: moment(layer.EndTime),
					Color: layer.Color
				};
			});
		}

		function buildShiftInfo(data) {
			data.Timeline = data.Timeline || {};
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
				change.Offset = calculateWidth(shiftInfo.timeWindowStart, change.Time, shiftInfo.timeWindowSeconds);
				change.RuleColor = !change.RuleColor ? "rgba(0,0,0,0.54)" : change.RuleColor;
				change.Color = change.RuleColor;
				change.click = function () {
					highlightThis(change);
				};
				return change;
			});
		}

		function highlightThis(change) {
			vm.diamonds.forEach(function (d) {
				d.highlight = false;
			});
			change.highlight = true;
			change.parent.isOpen = true;
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
			
			return [makeCard($translate.instant('BeforeShiftStart'), 'black', '0000-01-01T00:00:00', schedules[0].StartTime)]
				.concat(schedules.map(function (l) {
					var activityStart = moment(l.StartTime);
					var activityEnd = moment(l.EndTime);
					var header = l.Name + ' ' + activityStart.format('HH:mm') + ' - ' + activityEnd.format('HH:mm');
					return makeCard(header, l.Color, l.StartTime, l.EndTime);
				})).concat([makeCard($translate.instant('AfterShiftEnd'), 'black', schedules[schedules.length-1].EndTime, '2018-01-01T00:00:00')])
				.map(function (card) {
					card.Items = changes.filter(function (change) {
						var match = change.Time >= card.StartTime && change.Time < card.EndTime;
						if (match)
							change.parent = card;
						return match;
					});
					return card
				})
				.filter(function(card) {
					return card.Items.length > 0;
				});
			
		}

		function calculateWidth(startTime, endTime, totalSeconds) {
			var diff = moment(endTime).diff(moment(startTime), 'seconds');
			return (diff / totalSeconds) * 100 + '%';
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
