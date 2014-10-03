define([
		'knockout'
], function (
	ko
	) {

	return function () {
		var self = this;
		self.hub = undefined;
		self.recievedPings = ko.observableArray();
		self.expectedPongs = ko.observable(0);
		self.numberOfPings = ko.observable(100);
		self.messagesPerSecond = ko.observable(80);
		self.pongsLeft = ko.computed(function () {
			return self.expectedPongs() - self.recievedPings().length;
		});
		self.sentPings = ko.observable(0);
		self.isOnline = ko.observable(false);

		self.initialize = function (options) {
			if (options) {
				self.hub = options.messageBroker;
				if (options.signalR) {
					options.signalR.start()
						.done(function() {
						self.isOnline(true);
					});
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
	};
});

