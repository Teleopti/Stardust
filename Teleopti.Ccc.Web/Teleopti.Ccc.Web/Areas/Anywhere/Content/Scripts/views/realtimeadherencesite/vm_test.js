define(['buster', 'views/realtimeadherencesite/vm'], function (buster, viewModel) {
	return function () {
		
		buster.testCase("real time adherence site view model", {
			"should have no teams if none filled": function () {
				var vm = viewModel();
				assert.equals(vm.teams(), []);
			},

			"should fill site's team data": function () {
				var team1 = { Name: 'Green', Id: 'guid1', NumberOfAgents:5};
				var team2 = { Name: 'Red', Id: 'guid2', NumberOfAgents:6};
				var vm = viewModel();
				vm.fill([team1, team2]);

				assert.equals(vm.teams()[0].Id, team1.Id);
				assert.equals(vm.teams()[0].Name, team1.Name);
				assert.equals(vm.teams()[0].NumberOfAgents, team1.NumberOfAgents);
				assert.equals(vm.teams()[1].Id, team2.Id);
				assert.equals(vm.teams()[1].Name, team2.Name);
				assert.equals(vm.teams()[1].NumberOfAgents, team2.NumberOfAgents);
			},


			"should update number out of adherence": function () {
				var vm = viewModel();

				vm.fill([{ Name: 'Red', Id: 'guid1' }]);
				vm.update({ Id: 'guid1', OutOfAdherence: 1 });

				assert.equals(vm.teams()[0].OutOfAdherence(), 1);
			}

		});


		
	};
});