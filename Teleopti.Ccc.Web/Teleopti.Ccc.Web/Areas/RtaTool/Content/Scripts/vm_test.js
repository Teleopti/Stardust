define(['buster','vm','moment'], function (buster,viewmodel,moment) {
	return function () {

		buster.testCase("Rta Tool viewmodel", {
			"should create viewmodel": function() {

				var vm = new viewmodel();
				assert(vm);
			},
			"should state basic info for state": function () {

				var actualState = {};
				var vm = new viewmodel(function (state) {
					actualState = state;
				});

				vm.sendState();

				expect(actualState.AuthenticationKey).toEqual(vm.AuthenticationKey);
				expect(actualState.UserCode).toEqual('0085');
				expect(actualState.IsLoggedOn).toEqual(true);
				expect(actualState.SecondsInState).toEqual(0);
				expect(moment(actualState.TimeStamp).isValid()).toBe(true);
				expect(actualState.PlatformTypeId).toEqual('00000000-0000-0000-0000-000000000000');
				expect(actualState.SourceId).toEqual(1);
				expect(moment(actualState.BatchId).isValid()).toBe(true);
				expect(actualState.IsSnapshot).toBe(false);

			},


			"should send in call state when answer": function () {

				var actualState = {};
				var vm = new viewmodel(function(state) {
					actualState = state;
				});

				vm.answer();

				expect(actualState.StateCode).toEqual('InCall');
				expect(actualState.StateDescription).toEqual('InCall');

			},


			"should send ready call state when hang up": function () {

				var actualState = {};
				var vm = new viewmodel(function (state) {
					actualState = state;
				});

				vm.hangUp();

				expect(actualState.StateCode).toEqual('Ready');
				expect(actualState.StateDescription).toEqual('Ready');

			},

			"should send specified state on send": function () {

				var actualState = {};
				var vm = new viewmodel(function (state) {
					actualState = state;
				});

				vm.specifiedState("AUX1");
				vm.sendState();

				expect(actualState.StateCode).toEqual('AUX1');
				expect(actualState.StateDescription).toEqual('AUX1');
			},

			"should set IsLoggedOn to false on the state if set to false on the viewmodel": function () {
				var actualState = {};
				var vm = new viewmodel(function (state) {
					actualState = state;
				});
				vm.isLoggedOn(false);

				vm.sendState();

				expect(actualState.IsLoggedOn).toEqual(false);
			}
	});
	};
});