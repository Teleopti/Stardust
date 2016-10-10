
(function() {﻿
	'use strict';﻿﻿
	angular﻿.module('wfm.rta')﻿.controller('RtaHistoricalController', RtaHistoricalController);﻿﻿
	RtaHistoricalController.$inject = ['$stateParams', 'RtaService'];﻿﻿
	function RtaHistoricalController($stateParams, RtaService) {﻿
		var vm = this;

		var id = $stateParams.personId;

		RtaService.getAgentHistoricalData(id)
			.then(function(data) {
				var o = compareEarliestLastest(data.Schedule, data.OutOfAdherences);

				vm.personId = data.PersonId;
				vm.agentName = data.Name;
				vm.agentsFullSchedule = data.Schedule.map(function(layer) {
					return {
						Width: calculateWidth(layer.StartTime, layer.EndTime, o.totalSeconds),
						Offset: calculateWidth(o.start, layer.StartTime, o.totalSeconds),
						StartTime: layer.StartTime,
						EndTime: layer.EndTime
					};
				});

				vm.outOfAdherences = data.OutOfAdherences.map(function(ooa) {
					return {
						Width: calculateWidth(ooa.StartTime, ooa.EndTime, o.totalSeconds),
						Offset: calculateWidth(o.start, ooa.StartTime, o.totalSeconds),
						StartTime: ooa.StartTime,
						EndTime: ooa.EndTime
					};
				});

				vm.fullTimeline = buildTimeline(vm.agentsFullSchedule, vm.outOfAdherences);
			});

		function compareEarliestLastest(schedule, outOfAdherences) {
			if (schedule.length == 0 && outOfAdherences.length == 0)
				return {};

			var neededSchedule = {};
			var earliestScheduleLayer = earliest(schedule);
			var earliestOOA = earliest(outOfAdherences);
			var earliestStartTime;
			if (earliestScheduleLayer == null)
				earliestStartTime = earliestOOA;
			else if (earliestOOA == null)
				earliestStartTime = earliestScheduleLayer;
			else
				earliestStartTime = earliestScheduleLayer < earliestOOA ? earliestScheduleLayer : earliestOOA;

			var latestScheduleLayer = latest(schedule);
			var latestOOA = latest(outOfAdherences);
			var latestEndTime;
			if (latestScheduleLayer == null)
				latestEndTime = latestOOA;
			else if (latestOOA == null)
				latestEndTime = latestScheduleLayer;
			else
				latestEndTime = latestScheduleLayer > latestOOA ? latestScheduleLayer : latestOOA;

			var start = earliestStartTime.clone();
			start.startOf('hour');
			var end = latestEndTime.clone();
			end.endOf('hour');
			neededSchedule = {
				start: start,
				end: end,
				totalSeconds: latestEndTime.diff(earliestStartTime, 'seconds')
			}

			return neededSchedule;
		}

		function calculateWidth(startTime, endTime, totalSeconds) {
			var diff = moment(endTime).diff(moment(startTime), 'seconds');
			return (diff / totalSeconds) * 100 + '%';
		}

		function buildTimeline(schedule, outOfAdherences) {
			if (schedule.length == 0 && outOfAdherences.length == 0)
				return [];

			var timeline = [];

			var timePrepare = compareEarliestLastest(schedule, outOfAdherences);

			var currentMoment = timePrepare.start.clone();

			var totalHours = timePrepare.totalSeconds / 3600;
			var hourPercent = 100 / totalHours;
			for (var i = 0; i <= totalHours; i++) {
				timeline.push({
					Offset: hourPercent * i + '%',
					Time: currentMoment.format('HH:mm')
				});

				currentMoment.add(1, 'hour');
			}

			return timeline;
		}

		function earliest(arr) {
			return arr
				.map(function(el) {
					return moment(el.StartTime, 'YYYY-MM-DD HH:mm:ss');
				})
				.sort(sorter)[0];
		}

		function latest(arr) {
			return arr
				.map(function(el) {
					return moment(el.EndTime, 'YYYY-MM-DD HH:mm:ss');
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
