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

				assert.equals(vm.sites(), [site1, site2]);
			},

			"should update number out of adherence": function () {
				var site = { Id: 'guid1', OutOfAdherence: 1 };
				var vm = viewModel();
				vm.update(site);

				assert.equals(vm.sites()[0].OutOfAdherence(), 1);
			}

		});

	};
});