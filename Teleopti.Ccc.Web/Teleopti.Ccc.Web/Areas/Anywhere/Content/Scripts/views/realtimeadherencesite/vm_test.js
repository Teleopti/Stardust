define(['buster', 'views/realtimeadherencesite/vm'], function (buster, viewModel) {

	buster.testCase("real time adherence site view model", {
		
		"should fill site's team data": function () {
			var team1 = { name: 'Green', Id: 'guid1' };
			var team2 = { name: 'Red', Id: 'guid2' };
			var vm = viewModel();
			vm.fill([team1, team2]);

			assert.equals(vm.teams(), [team1, team2]);
		}
		
	});
});