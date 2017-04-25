
(function() {
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

		vm.ooaTooltipTime = function(time) {
			if (time == null)
				return '';

			return time.format('HH:mm:ss');
		};

		rtaService.getAgentHistoricalData(id)
			.then(function(data) {

				data.Schedules = data.Schedules || [];
				data.OutOfAdherences = data.OutOfAdherences || [];
				data.Changes = data.Changes || [];

				var shiftInfo = buildShiftInfo(data.Schedules, data.Now);

				vm.personId = data.PersonId;
				vm.agentName = data.AgentName;
				vm.date = moment(data.Now);

				vm.currentTimeOffset = calculateWidth(shiftInfo.timeWindowStart, data.Now, shiftInfo.timeWindowSeconds);

				vm.agentsFullSchedule = buildAgentsFullSchedule(data.Schedules, shiftInfo);

				vm.outOfAdherences = buildAgentOutOfAdherences(data, shiftInfo);

				vm.fullTimeline = buildTimeline(shiftInfo);

				vm.cards = mapChanges(data.Changes, data.Schedules);

				vm.diamonds = buildDiamonds(shiftInfo, data);

			});

		function buildAgentOutOfAdherences(data, shiftInfo) {
			return data.OutOfAdherences.map(function(ooa) {
				var startTime = moment(ooa.StartTime);
				var endTime = ooa.EndTime != null ? moment(ooa.EndTime) : null
				return {
					Width: calculateWidth(ooa.StartTime, ooa.EndTime != null ? ooa.EndTime : data.Now, shiftInfo.timeWindowSeconds),
					Offset: calculateWidth(shiftInfo.timeWindowStart, ooa.StartTime, shiftInfo.timeWindowSeconds),
					StartTime: startTime,
					EndTime: endTime
				};
			});
		}

		function buildAgentsFullSchedule(schedules, shiftInfo) {
			return schedules.map(function(layer) {
				return {
					Width: calculateWidth(layer.StartTime, layer.EndTime, shiftInfo.timeWindowSeconds),
					Offset: calculateWidth(shiftInfo.timeWindowStart, layer.StartTime, shiftInfo.timeWindowSeconds),
					StartTime: moment(layer.StartTime),
					EndTime: moment(layer.EndTime),
					Color: layer.Color
				};
			});
		}

		function buildShiftInfo(schedule, serverNow) {
			var result = {
				start: moment(serverNow).hour(8).minute(0).second(0),
				stop: moment(serverNow).hour(17).minute(0).second(0),
				totalSeconds: (17 - 8) * 3600
			};

			if (schedule.length > 0) {
				var earliestStartTime = earliest(schedule);
				var latestEndTime = latest(schedule);
				var start = earliestStartTime.clone();
				start.startOf('hour');
				var end = latestEndTime.clone();
				end.endOf('hour');

				result.start = start;
				result.end = end;
				result.totalSeconds = latestEndTime.diff(earliestStartTime, 'seconds');
			}

			result.timeWindowSeconds = result.totalSeconds + 7200;
			result.timeWindowStart = result.start.clone().add(-1, 'hour');

			return result;
		}

		function buildDiamonds(shiftInfo, data) {
			return data.Changes.map(function(change, i) {
				change.Offset = calculateWidth(shiftInfo.timeWindowStart, change.Time, shiftInfo.timeWindowSeconds);
				change.RuleColor = !change.RuleColor ? "rgba(0,0,0,0.54)" : change.RuleColor;
				change.Color = change.RuleColor;
				change.click = function() {
					highlightThis(change);
				};
				return change;
			});
		}

		function highlightThis(change) {
			vm.diamonds.forEach(function(d) {
				d.highlight = false;
			})
			change.highlight = true;
			change.parent.isOpen = true;
		}

		function mapChanges(changes, schedules) {
			return changes.reduce(function(arr, change) {
				var changeTime = moment(change.Time);
				var earliestStartTime = earliest(schedules);
				var latestEndTime = latest(schedules);

				var key;
				var cardColor = 'black';
				if (schedules.length === 0) {
					key = $translate.instant('NoShift');
				} else {
					if (changeTime.isBefore(earliestStartTime)) {
						key = $translate.instant('BeforeShiftStart');
					} else if (changeTime.isAfter(latestEndTime)) {
						key = $translate.instant('AfterShiftEnd');
					} else {
						var activityWhenChangeOccurred = schedules.find(function(layer) {
							return layer.StartTime <= change.Time && layer.EndTime > change.Time;
						});
						var activityStart = moment(activityWhenChangeOccurred.StartTime);
						var activityEnd = moment(activityWhenChangeOccurred.EndTime);
						cardColor = activityWhenChangeOccurred.Color;
						key = activityWhenChangeOccurred.Name + ' ' + activityStart.format('HH:mm') + ' - ' + activityEnd.format('HH:mm');
					}
				}
				var existing = arr.find(function(item) {
					return item.key === key;
				});
				if (existing == null) {
					existing = {
						key: key,
						Header: key,
						Items: [change],
						Color: cardColor,
						isOpen: $stateParams.open != "false"
					};
					arr.push(existing);
				} else {
					existing.Items.push(change);
				}

				change.parent = existing;

				return arr;
			}, []);
		}

		function calculateWidth(startTime, endTime, totalSeconds) {
			var diff = moment(endTime).diff(moment(startTime), 'seconds');
			return (diff / totalSeconds) * 100 + '%';
		}

		function buildTimeline(shiftInfo) {
			var timeline = [];
			var currentMoment = shiftInfo.start.clone().add(-1, 'hours');

			var totalHours = shiftInfo.totalSeconds / 3600 + 2;
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

		function earliest(arr) {
			return arr
				.map(function(el) {
					return moment(el.StartTime);
				})
				.sort(sorter)[0];
		}

		function latest(arr) {
			return arr
				.map(function(el) {
					return moment(el.EndTime);
				})
				.sort(reverseSorter)[0];
		}

		function sorter(first, second) {
			return first.diff(second, 'seconds');
		}

		function reverseSorter(first, second) {
			return sorter(second, first);
		}
	}
})();
