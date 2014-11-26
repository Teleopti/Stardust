define(['buster','vm','moment'], function (buster,viewmodel,moment) {
	return function () {

		buster.testCase("Rta Tool viewmodel", {
			"should create agents":function() {
				var agents = [
					{ name: 'Ashley Andeen', usercode: '0085' },
					{ name: 'Juancho Banaag', usercode: '0097' }
				];

				var vm = new viewmodel(function () { return agents; });

				expect(vm.agents()[0].name()).toEqual('Ashley Andeen');
				expect(vm.agents()[1].name()).toEqual('Juancho Banaag');
				expect(vm.agents()[0].usercode()).toEqual('0085');
				expect(vm.agents()[1].usercode()).toEqual('0097');
			}

	});
	};
});