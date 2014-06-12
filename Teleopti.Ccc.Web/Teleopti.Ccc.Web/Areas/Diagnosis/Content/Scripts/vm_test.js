
FakeMessagebrokerHub = function() {
	var self = this;

	self.server = new FakeSignalRServer();
	self.stuff = 'for test';


}


define(['buster','vm'], function (buster,viewmodel) {
	return function () {

		buster.testCase("Diagnosis viewmodel", {		
			"should start signalR": function () {

				//henke: måste byta ut sigR i Require för att kunna testa det?
				assert(true);

			},
			"viewmodel sends given number of ping-messages to messagebroker hub": function () {


				var callsToPing = new [];
				var stub = new FakeMessagebrokerHub();

				console.log('t6est');


				stub.server.pingWithId = function(id) {
					console.log('from stub....');
					callsToPing.push(id);
				};
				


				var target = new viewmodel();
				target.messageBroker = stub;

				console.log(target);
				target.sendGivenNumberOfMessages(100);

			}
		});

	};
});
