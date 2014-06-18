
define([
	'buster',
	'rta/scenario'
], function (
	buster,
	rtaScenario
	) {
	return function () {

		buster.testCase("My thing", {
			"should create scenario": function () {
				var scenario = new rtaScenario();
				assert.defined(scenario);
			},
			"should create right number of iterations":function() {
				var scenario = new rtaScenario();
				scenario.Configuration(JSON.stringify({
					ExternalLogOns: [2001],
					States: ["Ready", "OFF", "InCall"],
					StatesToSend: 100
				}, null, 4));
				assert.equals(scenario.Iterations().length, 100);
			}
		});

	};
});
