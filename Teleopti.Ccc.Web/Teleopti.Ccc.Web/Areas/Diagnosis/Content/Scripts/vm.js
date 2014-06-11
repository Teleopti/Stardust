define([
		'knockout',
		'jquery',
		'signalRHubs'
], function (
	ko,
	$,
	sigR
	) {

	return function() {

		var self = this;

		self.showInfo = function(info) {
			console.log('*******');
			console.log(info);
			console.log('*******');
		}

		self.sendTestMessage = function() {
			alert('sending testmessage');
		};

		self.startSignalR = function() {
			sigR.start();
		};

		self.subscribeToPing = function() {
			var hub = $.connection.MessageBrokerHub;
			hub.client.pong = function(someNumber) {
				self.showInfo('ping recieved from messagebroker');
				console.log(someNumber);
			};
		};

		self.sendMessageBrokerPing = function() {
			var hub = $.connection.MessageBrokerHub;
			self.showInfo('ping sent');
			hub.server.pingWithId(155);
		};

		self.checkMessageBrokerConnection = function() {
			var hub = $.connection.MessageBrokerHub;
			var stateConversion = { 0: 'connecting', 1: 'connected', 2: 'reconnecting', 3: 'disconnected' }
			var connectionInfo = 'SignalR connectionstatus: ' + stateConversion[hub.connection.state];
			self.showInfo(connectionInfo);			
		};

		self.testMessage = ko.observable('Send message');
	};

});

