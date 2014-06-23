define([
		'knockout',
		'jquery'
], function (
	ko,
	$
	) {

	return function() {

		var self = this;

		self.hub = '';
		self.connectionStates = { 0: 'connecting', 1: 'connected', 2: 'reconnecting', 3: 'disconnected' };
		self.connectionState = ko.observable(3);
		self.recievedPings = ko.observableArray();

		self.messageBrokerStatus = ko.observable(self.connectionStates[3]);
		self.isOnline = ko.computed(function() {
			return self.connectionState() == 1;
		});
		self.messageBrokerStatus = ko.computed(function() {
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

		self.sendPing = function(numberOfmessages) {
			self.recievedPings([]);
			for (var pingId = 0; pingId < numberOfmessages; ++pingId) {
				self.hub.server.pingWithId(pingId);
			}
		}

		self.updateMessageBrokerConnection = function () {
			var state = self.hub.connection.state;
			self.connectionState(state);
		};

		
	};

});

