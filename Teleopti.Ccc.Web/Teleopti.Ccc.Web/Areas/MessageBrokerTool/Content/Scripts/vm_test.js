define(['buster','vm'], function (buster,viewmodel) {
	return function() {
		buster.testCase("Diagnosis viewmodel", {
			"recieved pings should be cleared when we start sending new pings": function() {

				var vm = new viewmodel();
				//fake add some recieved pings:
				vm.recievedPings.push('1');
				vm.recievedPings.push('2');

				vm.sendAllPings();

				assert.equals(vm.recievedPings().length, 0);
			},

			"sending 20 pings should call hub to send 20 new messages": function () {
				var vm = new viewmodel();
				var sentPings = 0;
				var messagesPerSecond = 0;
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

				vm.messagesPerSecond(messagesPerSecond);
				vm.numberOfPings(20);
				vm.sendAllPings();

		
				assert.equals(sentPings, 20);
			}
	});
	};
});