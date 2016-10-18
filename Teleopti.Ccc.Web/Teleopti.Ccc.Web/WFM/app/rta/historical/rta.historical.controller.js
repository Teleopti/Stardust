
(function() {﻿
	'use strict';﻿﻿
	angular﻿.module('wfm.rta')﻿.controller('RtaHistoricalController', RtaHistoricalController);﻿﻿
	RtaHistoricalController.$inject = ['$stateParams', 'RtaService'];﻿﻿
	function RtaHistoricalController($stateParams, RtaService) {﻿
		var vm = this;

		var id = $stateParams.personId;

		RtaService.getAgentHistoricalData(id)
			.then(function(data) {
				var o = compareEarliestLatest(data.Schedules, data.OutOfAdherences, data.Now);

				vm.personId = data.PersonId;
				vm.agentName = data.AgentName;
				vm.date = moment(data.Now).format('YYYY-MM-DD');
				var totalSeconds = o.totalSeconds + 7200;
				var startOfShift = o.start != null ? o.start.clone().add(-1, 'hour') : moment().hour(7).minute(0).second(0);
				vm.agentsFullSchedule = data.Schedules.map(function(layer) {
					var startTime = moment(layer.StartTime);
					var endTime = moment(layer.EndTime);
					return {
						Width: calculateWidth(layer.StartTime, layer.EndTime, totalSeconds),
						Offset: calculateWidth(startOfShift, layer.StartTime, totalSeconds),
						StartTime: startTime,
						EndTime: endTime,
						Color: layer.Color,
						DisplayStartTime: startTime.format('YYYY-MM-DD HH:mm:ss'),
						DisplayEndTime: endTime.format('YYYY-MM-DD HH:mm:ss')
					};
				});

				vm.outOfAdherences = data.OutOfAdherences.map(function(ooa) {
					if (ooa.EndTime == null)
						ooa.EndTime = data.Now;

					var startTime = moment(ooa.StartTime);
					var endTime = ooa.EndTime != null ? moment(ooa.EndTime) : moment(data.Now);

					return {
						Width: calculateWidth(ooa.StartTime, ooa.EndTime, totalSeconds),
						Offset: calculateWidth(startOfShift, ooa.StartTime, totalSeconds),
						StartTime: startTime,
						EndTime: endTime,
						DisplayStartTime: startTime.format('YYYY-MM-DD HH:mm:ss'),
						DisplayEndTime: endTime.format('YYYY-MM-DD HH:mm:ss')
					};
				});

				vm.fullTimeline = buildTimeline(vm.agentsFullSchedule, vm.outOfAdherences);
			});

		function compareEarliestLatest(schedule, outOfAdherences, serverTime) {
			if (schedule.length === 0 && outOfAdherences.length === 0)
				return {};

			var earliestStartTime = earliest(schedule);
			var latestEndTime = latest(schedule, serverTime);

			var start = null;
			if (earliestStartTime != null) {
				start = earliestStartTime.clone();
				start.startOf('hour');
			}
			var end = null;
			if (latestEndTime != null) {
				end = latestEndTime.clone();
				end.endOf('hour');
			}

			return {
				start: start,
				end: end,
				totalSeconds: earliestStartTime != null && latestEndTime != null ? latestEndTime.diff(earliestStartTime, 'seconds') : null
			};
		}

		function calculateWidth(startTime, endTime, totalSeconds) {
			var diff = moment(endTime).diff(moment(startTime), 'seconds');
			return (diff / totalSeconds) * 100 + '%';
		}

		function buildTimeline(schedule, outOfAdherences) {
			if (schedule.length == 0 && outOfAdherences.length == 0)
				return [];

			var timeline = [];
			var timePrepare = compareEarliestLatest(schedule, outOfAdherences);
			if (timePrepare.start == null)
				return timeline;
			var currentMoment = timePrepare.start.clone().add(-1, 'hours');

			var totalHours = timePrepare.totalSeconds / 3600 + 2;
			var hourPercent = 100 / totalHours;
			for (var i = 0; i <= totalHours; i++) {
				var percent = hourPercent * i;
				if (percent >= 0 && percent < 100)
					timeline.push({
						Offset: percent + '%',
						Time: currentMoment.format('HH:mm')
					});

				currentMoment.add(1, 'hour');
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

		function latest(arr, serverTime) {
			return arr
				.map(function(el) {
					return el.EndTime != null ? moment(el.EndTime) : moment(serverTime);
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
