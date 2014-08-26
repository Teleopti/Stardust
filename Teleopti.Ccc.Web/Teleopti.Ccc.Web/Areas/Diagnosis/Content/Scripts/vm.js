define([
		'knockout'
], function (
	ko
	) {

	return function () {

		var self = this;
		self.hub = '';
		self.connectionStates = { 0: 'connecting', 1: 'connected', 2: 'reconnecting', 3: 'disconnected' };
		self.connectionState = ko.observable(3);
		self.recievedPings = ko.observableArray();
		self.messageBrokerStatus = ko.observable(self.connectionStates[3]);
		self.expectedPongs = ko.observable(0);
		self.numberOfPings = ko.observable(100);
		self.messagesPerSecond = ko.observable(80);
		self.hasRecievedAllPongs = ko.computed(function () {
			return self.expectedPongs() <= self.recievedPings().length;
		});
		self.pongsLeft = ko.computed(function () {
			return self.expectedPongs() - self.recievedPings().length;
		});
		self.sentPings = ko.observable(0);
		self.isOnline = ko.computed(function () {
			return self.connectionState() == 1;
		});
		self.messageBrokerStatus = ko.computed(function () {
			return self.connectionStates[self.connectionState()];
		});

		self.initialize = function (options) {
			if (options) {
				self.hub = options.messageBroker;
				if (options.signalR) {
					options.signalR.start();
				}

				if (self.hub && self.hub.client) {
					self.hub.client.pong = function (id) {
						self.recievedPings.push(id);

					}
				}
			}
		}

		self.sendAllPings = function () {
				self.sendPings(self.numberOfPings(), 0);
		};


		self.sendPings = function (pings) {
			self.recievedPings([]);
			self.sentPings(pings);
			self.expectedPongs(pings);

			if (self.hub && self.hub.server && self.hub.server.ping) {
				self.hub.server.ping(self.numberOfPings(), self.messagesPerSecond());

			}

		};

		self.updateMessageBrokerConnection = function () {
			var state = self.hub.connection.state;
			self.connectionState(state);
		};
	};

});

