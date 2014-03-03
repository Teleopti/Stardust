require(['buster', 'views/realtimeadherence/vm'], function (buster, viewModel) {

	buster.testCase("realTimeAdherence_viewModel", {
		"can create instance": function () {
			var vm = viewModel();
			assert.defined(vm);
		},
		
		"should have no sites if none filled": function() {
			var vm = viewModel();
			assert.equals(vm.sites(), []);
		},
		
		"can fill sites data": function () {
			var site1 = { name : 'Londin', Id : 'guid1' };
			var site2 = { name : 'PAris', Id : 'guid2' };
			var vm = viewModel();
			vm.fill([site1, site2]);

			assert.equals(vm.sites(), [site1, site2]);
		}
	});
});