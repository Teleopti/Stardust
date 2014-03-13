define(['buster', 'views/realtimeadherence/vm'], function (buster, viewModel) {
	return function () {

		buster.testCase("real time adherence view model", {
			"should have no sites if none filled": function () {
				var vm = viewModel();
				assert.equals(vm.sites(), []);
			},

			"should fill sites data": function () {
				var site1 = { Name: 'London', Id: 'guid1' };
				var site2 = { Name: 'Paris', Id: 'guid2' };
				var vm = viewModel();
				
				vm.fill([site1, site2]);

				assert.equals(vm.sites()[0].Id, "guid1");
				assert.equals(vm.sites()[0].Name, "London");
				assert.equals(vm.sites()[1].Id, "guid2");
				assert.equals(vm.sites()[1].Name, "Paris");
			},

			"should update number out of adherence": function () {
				var vm = viewModel();
				
				vm.update({ Id: 'guid1', OutOfAdherence: 1 });

				assert.equals(vm.sites()[0].OutOfAdherence(), 1);
			},

			"should update number out of adherence on existing site": function () {
				var vm = viewModel();
				vm.fill([{ Name: 'London', Id: 'guid1' }]);

				vm.update({ Id: 'guid1', OutOfAdherence: 1 });

				assert.equals(vm.sites().length, 1);
				assert.equals(vm.sites()[0].OutOfAdherence(), 1);
			}

		});

	};
});