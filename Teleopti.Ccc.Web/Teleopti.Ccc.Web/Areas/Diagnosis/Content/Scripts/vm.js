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
		self.numberOfPings = ko.observable(0);
		self.hasRecievedAllPongs = ko.computed(function () {
			return self.expectedPongs() <= self.recievedPings().length;
		});
		self.pongsLeft = ko.computed(function () {
			return self.expectedPongs() - self.recievedPings().length;
		});
		self.sentPings = ko.observable(0);
		self.batchSize = ko.observable(0);
		self.isOnline = ko.computed(function () {
			return self.connectionState() == 1;
		});
		self.interval_ms = ko.observable(0);
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

		self.intervalId = null;

		self.sendAllPings = function () {

			if (self.interval_ms() > 0) {
				self.intervalId = window.setInterval(self.sendNextPing, self.interval_ms());
			} else {
				self.sendPings(self.numberOfPings(), 0);
			}
		};

		self.sendNextPing = function () {
			if (self.sentPings() < self.numberOfPings()) {
				self.hub.server.pingWithId(self.sentPings());

				self.sentPings(self.sentPings() + 1);
				if (self.sentPings() >= self.numberOfPings() && self.intervalId) window.clearInterval(self.intervalId);

			}

		};

		self.sendBatch = function () {

			for (var i = 0; i < self.batchSize() ; ++i) {
				if (self.sentPings() < self.numberOfPings()) {
					self.sendNextPing();
				}
			}
		};

		self.sendPings = function (pings) {
			self.recievedPings([]);
			self.sentPings(0);
			self.expectedPongs(pings);

			for (var pingId = 0; pingId < pings; ++pingId) {
				self.sendNextPing();
			}
		};

		self.updateMessageBrokerConnection = function () {
			var state = self.hub.connection.state;
			self.connectionState(state);
		};
	};

});

