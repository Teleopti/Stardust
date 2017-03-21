(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.factory('rtaAgentsBuildService', rtaAgentsBuildService);

	rtaAgentsBuildService.$inject = ['rtaFormatService'];

	function rtaAgentsBuildService(rtaFormatService) {

		var service = {
			buildAgentState: buildAgentState
		}

		return service;

		function buildAgentState(now, state) {
			state.Shift = state.Shift || [];
			state.OutOfAdherences = state.OutOfAdherences || [];
			var timeInfo = {
				time: now,
				windowStart: now.clone().add(-1, 'hours'),
				windowEnd: now.clone().add(3, 'hours')
			};

			return {
				PersonId: state.PersonId,
				Name: state.Name,
				SiteAndTeamName: state.SiteName + '/' + state.TeamName,
				TeamName: state.TeamName,
				SiteName: state.SiteName,
				TeamId: state.TeamId,
				State: state.State,
				Activity: state.Activity,
				NextActivity: state.NextActivity,
				NextActivityStartTime: state.NextActivityStartTime,
				Rule: state.Rule,
				Color: state.Color,
				TimeInState: state.TimeInState,
				TimeInAlarm: getTimeInAlarm(state),
				TimeInRule: state.TimeInAlarm ? state.TimeInRule : null,
				TimeOutOfAdherence: getTimeOutOfAdherence(state, timeInfo),
				OutOfAdherences: getOutOfAdherences(state, timeInfo),
				ShiftTimeBar: getShiftTimeBar(state),
				Shift: getShift(state, timeInfo)
			};
		}

		function getTimeInAlarm(state) {
			if (state.TimeInAlarm !== null)
				return rtaFormatService.formatDuration(state.TimeInAlarm);
		}

		function getTimeOutOfAdherence(state, timeInfo) {
			if (state.OutOfAdherences.length > 0) {
				var lastOOA = state.OutOfAdherences[state.OutOfAdherences.length - 1];
				if (lastOOA.EndTime == null) {
					var seconds = moment(timeInfo.time).diff(moment(lastOOA.StartTime), 'seconds');
					return rtaFormatService.formatDuration(seconds);
				}
			}
		}

		function getOutOfAdherences(state, timeInfo) {
			return state.OutOfAdherences
				.filter(function (t) { return t.EndTime == null || moment(t.EndTime) > timeInfo.windowStart; })
				.map(function (t) {
					var endTime = t.EndTime || timeInfo.time;
					return {
						Offset: Math.max(rtaFormatService.timeToPercent(timeInfo.time, t.StartTime), 0) + '%',
						Width: Math.min(rtaFormatService.timePeriodToPercent(timeInfo.windowStart, t.StartTime, endTime), 100) + "%",
						StartTime: moment(t.StartTime).format('HH:mm:ss'),
						EndTime: t.EndTime ? moment(t.EndTime).format('HH:mm:ss') : null,
					};
				});
		}

		function getShiftTimeBar(state) {
			var percentForTimeBar = function (seconds) { return Math.min(rtaFormatService.secondsToPercent(seconds), 25); }
			return (state.TimeInAlarm ? percentForTimeBar(state.TimeInRule) : 0) + "%";
		}

		function getShift(state, timeInfo) {
			return state.Shift
				.filter(function (layer) { return timeInfo.windowStart < moment(layer.EndTime) && timeInfo.windowEnd > moment(layer.StartTime); })
				.map(function (s) {
					return {
						Color: s.Color,
						Offset: Math.max(rtaFormatService.timeToPercent(timeInfo.time, s.StartTime), 0) + '%',
						Width: Math.min(rtaFormatService.timePeriodToPercent(timeInfo.windowStart, s.StartTime, s.EndTime), 100) + "%",
						Name: s.Name,
						Class: getClassForActivity(timeInfo.time, s.StartTime, s.EndTime)
					};
				});
		}
		

		function getClassForActivity(currentTime, startTime, endTime) {
			var now = moment(currentTime).unix(),
				start = moment(startTime).unix(),
				end = moment(endTime).unix();

			if (now < start)
				return 'next-activity';
			else if (now > end)
				return 'previous-activity';
			return 'current-activity';
		}
		
	};
})();
