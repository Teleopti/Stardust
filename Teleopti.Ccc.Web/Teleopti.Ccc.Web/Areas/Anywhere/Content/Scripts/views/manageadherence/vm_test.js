define(['buster', 'views/manageadherence/vm'], function(buster, viewModel) {
	return function() {
		buster.testCase("=>manage adherence viewmodel", {
			"should create viewmodel": function() {
				var vm = new viewModel();
				assert(vm);
			},

			"should set view options": function () {
				var vm = new viewModel();
				vm.setViewOptions({ id: 'guid1' });
				assert.equals(vm.PersonId(), 'guid1');
			}
		});
	};
});