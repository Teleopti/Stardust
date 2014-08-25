

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

				vm.sendAllPings();

				assert.equals(vm.recievedPings().length, 0);
			},

			"// sending 20 pings should send 20 pings with unique ids for each ping": function() {
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

				vm.numberOfPings(20);
				vm.sendAllPings();
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

			},

			"when sending a message number, sent messages should be equal to the sent number":function() {
				var vm = new viewmodel();
				var messagebroker = {
					server: {
						pingWithId: function () {
						}
					}
				}

				vm.initialize({
					messageBroker: messagebroker
				});

				vm.numberOfPings(2);
				vm.sendAllPings();
			
				assert.equals(vm.sentPings(), 2);
			},

			"// send batch should send a given number of messages until it reaches total number of messages" : function() {

			
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


				vm.batchSize(5);
				vm.numberOfPings(10);

				vm.sendBatch();
				var expected = [0, 1, 2, 3, 4];
				assert.equals(sentPings, expected);

				vm.sendBatch();
				expected = [0, 1, 2, 3, 4, 5, 6 ,7, 8 ,9];
				assert.equals(sentPings, expected);

				vm.sendBatch();
				assert.equals(sentPings, expected);

			},

			"// send when delay is set should send message each time the interval ticks": function() {

				var expectedIntervalTimeMs = 0;
				var fakeTickInterval = function (){}
				window.setInterval = function (callBack, timeMs) {
					expectedIntervalTimeMs = timeMs;
					fakeTickInterval = callBack;
				}

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

				vm.numberOfPings(3);
				vm.interval_ms(5);
				vm.sendAllPings();

				assert.equals(expectedIntervalTimeMs, 5);
				assert.equals(sentPings.length, 0); //should not have sent any messages yet

				//fake interval tick:
				fakeTickInterval();
				fakeTickInterval();
				fakeTickInterval();
				assert.equals(sentPings.length, 3);
			},

			"// sending delayed messages should stop when maximum is reached": function () {

				var fakeTickInterval = function () { }
				var timerId = 'myId';
				var clearedId = '';
				var timerIsRunning = function() {
					return clearedId != timerId;
				};
				window.setInterval = function (callBack) {
					fakeTickInterval = callBack;
					return timerId;
				}

				window.clearInterval = function(id) {
					clearedId = id;
				};

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

				vm.numberOfPings(2);
				vm.interval_ms(1);
				vm.sendAllPings();
				assert(timerIsRunning()); 

				fakeTickInterval();
				assert(timerIsRunning()); 

				fakeTickInterval();
				assert.equals(sentPings.length, 2);

				assert(!timerIsRunning());

				fakeTickInterval();
				assert.equals(sentPings.length, 2);
			},

	});

	};
});

			
