
define([
	'buster',
	'../vm',
	'rta/scenario'
], function (
	buster,
	viewModel,
	rtaScenario
	) {
	return function () {

		buster.testCase("Rta Scenario", {

			"should create instance": function() {
				var vm = new viewModel();
				assert(vm);
			},

			"should have default configuration" : function() {
				var vm = new viewModel();
				vm.SelectScenarioByName("Real Time Adherence Load Test");

				var configuration = JSON.parse(vm.Configuration());

				assert(configuration.PlatformTypeId);
				assert(configuration.SourceId);

				assert.equals(configuration.Persons.length, 1);
				assert.equals(configuration.Persons[0].ExternalLogOn, "2001");
				assert.equals(configuration.Persons[0].PersonId, "B46A2588-8861-42E3-AB03-9B5E015B257C");
				assert.equals(configuration.States.length, 2);
				assert.contains(configuration.States, "Ready");
				assert.contains(configuration.States, "OFF");
				assert.equals(configuration.ExpectedEndingStateGroup, "Logged off");
			},

			"should iterate combinations of persons and states" : function() {
				var vm = new viewModel();
				vm.SelectScenarioByName("Real Time Adherence Load Test");

				var configuration = JSON.parse(vm.Configuration());
				configuration.Persons = [
				{
					ExternalLogOn: "1",
					PersonId: "1-1"
				}, 
				{
					ExternalLogOn: "2",
					PersonId: "2-2"
				}];
				configuration.States = ["A", "B"];
				vm.Configuration(JSON.stringify(configuration));

				assert.match(vm.RunButtonText(), "4");
			},

			"should track progress by persons in expected ending state group": function() {
				var vm = new viewModel();
				vm.SelectScenarioByName("Real Time Adherence Load Test");

				var configuration = JSON.parse(vm.Configuration());
				configuration.Persons = [
				{
					ExternalLogOn: "1",
					PersonId: "1-1"
				},
				{
					ExternalLogOn: "2",
					PersonId: "2-2"
				}];
				configuration.States = ["A", "B"];
				configuration.ExpectedEndingStateGroup = "Toa rast";
				vm.Configuration(JSON.stringify(configuration));

				assert.equals(vm.ProgressItems()[0].Text, "Persons in expected ending state group");
				assert.equals(vm.ProgressItems()[0].Target(), 2);
			}
		});
	};
});
