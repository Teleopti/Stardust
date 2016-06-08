(function() {

    'use strict';

    angular.module('wfm.signalR',[])
        .constant('$', window.jQuery)
        .service('signalRSVC', ['$', '$timeout', signalRService]);

    function signalRService($, $timeout) {
        var service = this;
        var hub = $.connection.MessageBrokerHub;
        var pendingMessage = [];
        var messageHandlingTimeout = null;

        service.subscribe = function (options, messsageHandler) {
            hub.client.onEventMessage = function(message) {
                messsageHandler(message);
            }

            setNegotiationLocation();

            connectToServerAddSubscription(options);
        }

        service.subscribeBatchMessage = function(options, messageHandler, timeout) {
            hub.client.onEventMessage = function(message) {
                pendingMessage.push(message);
                messageHandlingTimeout !== null && $timeout.cancel(messageHandlingTimeout);

                messageHandlingTimeout = $timeout(function() {
                    messageHandler(pendingMessage);
                    resetPendingMessages();
                }, timeout);
            };

            setNegotiationLocation();

            connectToServerAddSubscription(options);
        }

        function resetPendingMessages() {
            pendingMessage = [];
            messageHandlingTimeout = null;
        }

        function setNegotiationLocation() {
            $.connection.hub.url = '../signalr';
        }

        function connectToServerAddSubscription(options) {
            $.connection.hub.start().done(function() {
                hub.server.addSubscription(options);
            }).fail(function(error) {
                //todo: notification.notify();
            });
        }

    }
})();