define([
		'jquery',
		'noext!../../../../signalr/hubs'
	], function (
		$,
		signalrHubs
	) {
		
		var startPromise;

		var subscription = function (options) {

			var self = this;

			this.callback = options.callback;
			this.serverSubscribeMethod = options.serverSubscribeMethod;
			
			this.incomingData = function (data) {
				//throw "why cant I see this?";
				if (self.callback) {
					try {
						self.callback(data);
					} catch(e) {
						// why is signalr eating my exceptions?
						console.log(e);
						throw e;
					} 
				}
			};

			options.clientIncomingMethodSetter(this.incomingData);

			this.subscribe = function() {
				var argumentsArray = Array.prototype.slice.call(arguments);
				var serverArguments = argumentsArray.slice(0, arguments.length - 1);
				var callbackArgument = arguments[arguments.length - 1];
				self.callback = callbackArgument;
				startPromise.done(function() {
					self.serverSubscribeMethod.apply(self, serverArguments);
				});
			};

		};

		var subscriptions = [];
		var teamScheduleHub = $.connection.teamScheduleHub;
		var personScheduleHub = $.connection.personScheduleHub;

		subscriptions.push(new subscription({
			serverSubscribeMethod: teamScheduleHub.server.subscribeTeamSchedule,
			clientIncomingMethodSetter: function(method) {
				teamScheduleHub.client.incomingTeamSchedule = method;
			}
		}));

		subscriptions.push(new subscription({
			serverSubscribeMethod: personScheduleHub.server.subscribePersonSchedule,
			clientIncomingMethodSetter: function(method) {
				personScheduleHub.client.incomingPersonSchedule = method;
			}
		}));

		var start = function () {
			$.connection.hub.url = 'signalr';
			startPromise = $.connection.hub.start();
			return startPromise;
		};

		return {
			start: start,
			subscribeTeamSchedule: subscriptions[0].subscribe,
			subscribePersonSchedule: subscriptions[1].subscribe,
		};

	});
