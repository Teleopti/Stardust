define(['buster','healthvm'], function (buster,viewmodel) {
	return function () {
		Teleopti = {};
		buster.testCase("Health check viewmodel", {
			"supplied bus check function should be called when starting bus check": function() {

				var result = false;
				var vm = new viewmodel();

				vm.initialize({
					checkBus: function() {
						result = true;
					},
					signalR: { start: function() { return { done: function() {} } } }
				});

				vm.checkBus();

				assert.equals(true, result);
			},
			"should subscribe for bus diagnostics message": function() {
				var result;
				var vm = new viewmodel();

				var fakePromise = { done: function(callback) {
					callback();
				} };

				vm.initialize({
					checkBus: function() { },
					signalR: { start: function() {
							return fakePromise;
						}
					},
					messageBroker: {server: {addSubscription: function(s) {
						result = s;
						return {done: function (){}}
					}}}
				});

				assert.equals('ITeleoptiDiagnosticsInformation', result.DomainType);
			}
		});
	};
});