

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
				vm.initialize();

				//fake add some recieved pings:
				vm.recievedPings.push('1');
				vm.recievedPings.push('2');

				vm.sendPing(0);

				assert.equals(vm.recievedPings().length, 0);
			},

			"sending 20 pings should send 20 pings with unique ids for each ping": function() {
				var vm = new viewmodel();
				var sentPings = [];
				var messagebroker = {
					server: {
						pingWithId: function (id) {
							sentPings.push(id);
						}
					}
				}

				vm.initialize({
					messageBroker: messagebroker
				});

				vm.sendPing(20);
				var sortedById = sentPings.sort();

				var duplicates = [];
				for (var i = 0; i < sortedById.length - 1; i++) {
					if (sortedById[i + 1] == sortedById[i]) {
						duplicates.push(sortedById[i]);
					}
				}

				assert.equals(sentPings.length, 20);
				assert.equals(duplicates.length, 0); //no duplicates

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

			
