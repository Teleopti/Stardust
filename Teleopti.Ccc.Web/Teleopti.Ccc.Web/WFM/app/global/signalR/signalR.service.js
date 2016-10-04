(function() {
    'use strict';

    angular
        .module('wfm.signalR', [])
        .constant('$', window.jQuery)
        .service('signalRSVC', signalRSVC);

    signalRSVC.$inject = ['$', '$timeout'];

    function signalRSVC($, $timeout) {
      var hub = $.connection.MessageBrokerHub;
      var pendingMessage = [];
      var messageHandlingTimeout = null;
      var messageHandlers = [];

      this.subscribe = subscribe;
      this.subscribeBatchMessage = subscribeBatchMessage;

      function subscribe(options, messsageHandler) {
        messageHandlers[options.DomainType] = messsageHandler;
        hub.client.onEventMessage = function (message) {
          messageHandlers[message.DomainType](message);
        }
          setNegotiationLocation();
          connectToServerAddSubscription(options);
      }

      function subscribeBatchMessage(options, messageHandler, timeout) {
          $.connection.hub.stop();
          hub.client.onEventMessage = function(message) {
              pendingMessage.push(message);
              messageHandlingTimeout !== null && $timeout.cancel(messageHandlingTimeout);

              messageHandlingTimeout = $timeout(function() {
                  messageHandler(pendingMessage);
                  resetPendingMessages();
              }, timeout);
          }
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
