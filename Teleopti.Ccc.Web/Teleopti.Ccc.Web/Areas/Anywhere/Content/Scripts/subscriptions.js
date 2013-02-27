define([
		'jquery',
		'noext!../../../../signalr/hubs'
	], function (
		$,
		signalrHubs
	) {
//		
//		var subscription = function (options) {

//			var self = this;

//			this.callback = options.callback;
//			this.serverSubscribe = options.serverSubscribe;

//			this.incomingData = function (data) {
//				if (callback) callback(data);
//			};

//			this.subscribe = function () {
//				self.callback = arguments[arguments.length - 1];
//				self.serverSubscribe.apply(self, arguments.slice(0, arguments.length - 2));
//			};

//		};

//		var subscriptions = [];

		var startPromise;
		
		var teamScheduleHub = $.connection.teamScheduleHub;
		var personScheduleHub = $.connection.personScheduleHub;

//		subscriptions.push(new subscription({
//			serverSubscribe: teamScheduleHub.server.subscribeTeamSchedule,
//		}));

		var incomingTeamScheduleCallback = null;
		var incomingTeamSchedule = function (data) {
			if (incomingTeamScheduleCallback)
				incomingTeamScheduleCallback(data);
		};
		var subscribeTeamSchedule = function (teamId, date, callback) {
			incomingTeamScheduleCallback = callback;
			startPromise.done(function() {
				teamScheduleHub.server.subscribeTeamSchedule(teamId, date);
			});
		};

		var incomingPersonScheduleCallback = null;
		var incomingPersonSchedule = function (data) {
			if (incomingPersonScheduleCallback)
				incomingPersonScheduleCallback(data);
		};
		var subscribePersonSchedule = function (personId, date, callback) {
			incomingPersonScheduleCallback = callback;
			startPromise.done(function() {
				personScheduleHub.server.subscribePersonSchedule(personId, date);
			});
		};

		var start = function () {
			$.connection.hub.url = 'signalr';

//			for (var i = 0; i < subscriptions.length; i++) {
//				var sub = subscriptions[i];
//			}
			teamScheduleHub.client.incomingTeamSchedule = incomingTeamSchedule;
			personScheduleHub.client.incomingPersonSchedule = incomingPersonSchedule;
			
			startPromise = $.connection.hub.start();
			return startPromise;
		};

		return {
			start: start,
			subscribeTeamSchedule: subscribeTeamSchedule,
			subscribePersonSchedule: subscribePersonSchedule,
		};

	});
