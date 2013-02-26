define([
		'jquery',
		'noext!../../../../signalr/hubs'
	], function (
		$,
		signalrHubs
	) {

		var teamScheduleHub = $.connection.teamScheduleHub;
		var personScheduleHub = $.connection.personScheduleHub;

		var incomingTeamScheduleCallback = null;
		var incomingTeamSchedule = function (data) {
			if (incomingTeamScheduleCallback)
				incomingTeamScheduleCallback(data);
		};
		var subscribeTeamSchedule = function (teamId, date, callback) {
			incomingTeamScheduleCallback = callback;
			teamScheduleHub.server.subscribeTeamSchedule(teamId, date);
		};

		var start = function () {
			$.connection.hub.url = 'signalr';
			teamScheduleHub.client.incomingTeamSchedule = incomingTeamSchedule;
			var promise = $.connection.hub.start();
			return promise;
		};

		return {
			start: start,
			subscribeTeamSchedule: subscribeTeamSchedule
		};

	});
