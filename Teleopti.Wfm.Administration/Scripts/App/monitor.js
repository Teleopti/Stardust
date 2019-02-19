(function () {
	'use strict';

	new Vue({
		el: '#steps',
		data: {
			steps: []
		},
		mounted() {
			var vm = this;
			axios.get('/monitor/list')
				.then(monitorSteps => {
				monitorSteps.data.forEach(function (step) {
					vm.steps.push({
						stepName: step,
						stepSuccess : false,
						output: 'Loading...'
					});
					axios.get('/monitor/check/' + step)
						.then(result => {
						var stepToUse = vm.findStep(step);
					stepToUse.output = result.data;
					stepToUse.stepSuccess = true;
				})
				.catch(error => {
						var stepToUse = vm.findStep(step);
					stepToUse.output = error.response.data;
				})
				});
		});
		},
		methods : {
			outputColor : function(value){
				return value ? 'stepSuccess' : 'stepFailure';
			},
			findStep : function(stepName){
				return this.steps.find(function (stepToFind) {
					return stepToFind.stepName === stepName;
				});
			}
		}
	});
})();