define(['buster', 'views/realtimeadherenceteams/vm'], function (buster, viewModel) {
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
			},

			"should update number out of adherence on existing team": function () {
				var vm = viewModel();
				vm.fill([{ Name: 'Team Green', Id: 'guid1' }]);
				vm.update({ Id: 'guid1', OutOfAdherence: 1 });

				assert.equals(vm.teams().length, 1);
				assert.equals(vm.teams()[0].OutOfAdherence(), 1);
			},

			"should not add when update if there is no team": function () {
				var vm = viewModel();
				vm.update({ Id: 'guid1', OutOfAdherence: 1 });

				assert.equals(vm.teams().length, 0);
			},

			"should set number of agents when fill": function () {
				var expected = 37;
				var vm = viewModel();
				var team = { NumberOfAgents: expected };

				vm.fill([team]);

				assert.equals(vm.teams()[0].NumberOfAgents, expected);
			},

			"should do update from notification": function () {
				var vm = viewModel();
				var notification = {
					DomainId: 'theguid',
					BinaryData: '{"OutOfAdherence":2}',
				};
				var team = { Id: "theguid" };
				vm.fill([team]);

				vm.updateFromNotification(notification);

				assert.equals(vm.teams()[0].OutOfAdherence(), 2);
			},
			"should have resources" : function () {
				var vm = viewModel();

				assert.defined(vm.resources);
			},
			"should get site name" : function() {
				var vm = viewModel();
				var site = { Name: Math.random()};

				vm.setSiteName(site);

				assert.equals(vm.siteName(), site.Name);
			}			
		});		
	};
});