(function(angular) {

	"use strict";

	angular.module("wfm.teamSchedule")
		.constant('$', window.jQuery)
		.service("SignalR", ['$','$timeout', signalRService]);


	function signalRService($, $timeout) {
		var service = this;
		var hub = $.connection.MessageBrokerHub;
		var pendingMessages = [];
		var messagesHandlingTimeout = null;

		service.subscribe = function(options, messageHandler) {
			hub.client.onEventMessage = function(message) { messageHandler(message); };
			$.connection.hub.start()
				.done(function() { hub.server.addSubscription(options); })
				.fail(function() {});
		};

		service.subscribeBatchMessage = function (options, messageHandler, timeout) {

			hub.client.onEventMessage = function (message) {
				pendingMessages.push(message);
				messagesHandlingTimeout !== null && $timeout.cancel(messagesHandlingTimeout);
				
				messagesHandlingTimeout = $timeout(function () {
					messageHandler(pendingMessages);
					resetPendingMessages();
				}, timeout);
			};

			negotiatesFromCurrentLocation();
			$.connection.hub.start()
				.done(function () { hub.server.addSubscription(options); })
				.fail(function () { });
		};

		function resetPendingMessages() {
			pendingMessages = [];
			messagesHandlingTimeout = null;
		}

		function negotiatesFromCurrentLocation() {
			$.connection.hub.url = 'signalr';
		}
	};

})(window.angular);
