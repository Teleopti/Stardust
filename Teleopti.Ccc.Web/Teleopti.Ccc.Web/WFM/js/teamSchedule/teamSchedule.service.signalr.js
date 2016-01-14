(function(angular) {

	"use strict";

	angular.module("wfm.teamSchedule")
		.constant('$', window.jQuery)
		.service("SignalR", ['$', signalRService]);


	function signalRService($) {
		var service = this;
		var hub = $.connection.MessageBrokerHub;
		
		service.subscribe = function(options, messageHandler) {
			hub.client.onEventMessage = function(message) { messageHandler(message); };
			$.connection.hub.start()
				.done(function() { hub.server.addSubscription(options); })
				.fail(function() {});
		};


	};

})(window.angular);
