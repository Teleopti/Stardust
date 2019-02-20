(function () {
	'use strict';

	new Vue({
		el: '#steps',
		data: {
			steps: []
		},
		mounted: function() {
			var vm = this;
			axios.get('./status/list')
				.then(function (statusSteps) {
					statusSteps.data.forEach(function (step) {
						var newStep = {
							stepName: step.Name,
							stepSuccess: false,
							output: 'Loading...',
							url: step.Url,
							description : step.Description
						};
						vm.steps.push(newStep);
						axios.get(step.Url)
							.then(function (result) {
								newStep.output = result.data;
								newStep.stepSuccess = true;
							})
							.catch(function (error) {
								newStep.output = error.response.data;
							})
					});
				});
		},
		methods: {
			outputColor: function (value) {
				return value ? 'stepSuccess' : 'stepFailure';
			}
		}
	});
})();