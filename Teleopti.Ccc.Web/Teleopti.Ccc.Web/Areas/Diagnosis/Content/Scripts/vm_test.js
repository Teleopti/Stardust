

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

			"recieved pings should be cleared when we start sending new pings": function() {

				var vm = new viewmodel();

				vm.initialize({
					signalR: {
						start: function () {
							signalRStarted = true;
						}
					}
				});
				//fake add some recieved pings:
				vm.recievedPings.push('1');
				vm.recievedPings.push('2');

				vm.sendAllPings();

				assert.equals(vm.recievedPings().length, 0);
			},

			"sending 20 pings should call hub to send 20 new messages": function () {
				var vm = new viewmodel();
				var sentPings = 0;
				var messagebroker = {
					server: {
						ping: function (numberOfMessages) {
							sentPings = numberOfMessages;
						}
					}
				}

				vm.initialize({
					messageBroker: messagebroker
				});

				vm.numberOfPings(20);
				vm.sendAllPings();

		
				assert.equals(sentPings, 20);


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
			},

			"finished should be true when all sent pings are recieved as pong": function() {
				var vm = new viewmodel();
				var sentPings = [];

				// we need a client so we can fake the pong from messagebroker
				var client = {};

				var messagebroker = {
					server: {
						pingWithId: function (id) {
							sentPings.push(id);
						}
					},
					client: client
				}

				vm.initialize({
					messageBroker: messagebroker
				});
				vm.numberOfPings(10);
				vm.sendAllPings();
				assert(!vm.hasRecievedAllPongs());

				assert.equals(vm.pongsLeft(), 10);

				for (i = 0; i < 6; i++) {
					client.pong(i);
				}

				assert(!vm.hasRecievedAllPongs());
				assert.equals(vm.pongsLeft(), 4);

				for (i = 6; i < 10; i++) {
					client.pong(i);
				}
				assert(vm.hasRecievedAllPongs());
				assert.equals(vm.pongsLeft(), 0);

			}

	});

	};
});

			
