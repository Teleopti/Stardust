define(['buster','vm'], function (buster,viewmodel) {
	return function() {
		buster.testCase("Diagnosis viewmodel", {
			"should create viewmodel": function() {

				var vm = new viewmodel();
				assert(vm);
			}
	});
	};
});