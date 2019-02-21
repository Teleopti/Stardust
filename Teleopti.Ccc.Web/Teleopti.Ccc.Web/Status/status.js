(function () {
	'use strict';

	new Vue({
		el: '#steps',
		data: {
			steps: [],
			stepsAreLoaded: false
		},
		mounted: function() {
			var vm = this;
			axios.get('../status/list')
				.then(function (statusSteps) {
					vm.stepsAreLoaded = true;
					statusSteps.data.forEach(function (step) {
						var newStep = {
							stepName: step.Name,
							stepSuccess: false,
							output: 'Verifying...',
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