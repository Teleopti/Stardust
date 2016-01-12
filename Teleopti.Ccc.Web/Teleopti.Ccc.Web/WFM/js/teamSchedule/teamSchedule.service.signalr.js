"use strict";

angular.module("wfm.teamSchedule")
	.constant('$', window.jQuery)
	.service("SignalR", [
	function() {
		return {
			subscribe: function (options, eventHandler, errorHandler) {
				var hub = $.connection.MessageBrokerHub;
				hub.client.onEventMessage = function (message) {
					eventHandler(message);
				};

				$.connection.hub.start().done(function () {
					hub.server.addSubscription(options);
				}).fail(function (message) {
					errorHandler(message);
				});
			}
		};
	}
]);