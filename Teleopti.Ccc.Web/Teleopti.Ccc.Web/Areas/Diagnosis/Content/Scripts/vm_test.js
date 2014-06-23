

define(['buster','vm'], function (buster,viewmodel) {
	return function() {
		buster.testCase("Diagnosis viewmodel", {
			"initialize should start signalR": function() {

				var vm = new viewmodel();
				var signalRStarted = false;

				vm.initialize({
					signalR: {
						start: function() {
							signalRStarted = true;
						}
					}
				});

				assert(signalRStarted);
			},

			"viewmodel sends given number of ping-messages to messagebroker hub": function() {

				var vm = new viewmodel();
				var sentPings = [];
				var messagebroker = {
					server: {
						pingWithId: function(id) {
							sentPings.push(id);
						}
					}
				}

				vm.initialize({
					messageBroker: messagebroker
				});

				vm.sendPing(100);

				assert.equals(sentPings.length, 100);
			},


			"recieved pings should be cleared when we start sending new pings": function() {

				var vm = new viewmodel();
				vm.initialize();

				//fake add some recieved pings:
				vm.recievedPings.push('1');
				vm.recievedPings.push('2');

				vm.sendPing(0);

				assert.equals(vm.recievedPings().length, 0);
			},

			"viewmodel subscribe to pong from messagebroker": function() {
				var vm = new viewmodel();

				var client = {};
				var messagebroker = {
					client: client
				}

				vm.initialize({
					messageBroker: messagebroker
				});

				client.pong(1);
				client.pong(2);

				assert.equals(vm.recievedPings().length, 2);
			},

			"messagebroker default connectionstate should be disconnected": function() {
				var vm = new viewmodel();

				//default is disconnected, state 3
				var disconnectedDescription = vm.connectionStates[3];

				var messagebroker = {
					connection: {
						state: 1 //could be anything since we havent checked the connection...
					}
				}

				vm.initialize({
					messageBroker: messagebroker
				});

				assert.equals(vm.messageBrokerStatus(), disconnectedDescription);
				assert(!vm.isOnline());


			},

			"messagebroker connectionstatus should be connected when broker is working": function() {

				var vm = new viewmodel();
				var connectedBroker = {
					connection: {
						state: 1
					}
				}

				vm.initialize({ messageBroker: connectedBroker });
				vm.updateMessageBrokerConnection();

				assert.equals(vm.messageBrokerStatus(), vm.connectionStates[1]);
				assert(vm.isOnline());
			}


	});

	};
});

			
