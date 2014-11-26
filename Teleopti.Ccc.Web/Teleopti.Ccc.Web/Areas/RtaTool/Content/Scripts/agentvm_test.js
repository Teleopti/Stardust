define(['buster', 'agentvm', 'moment'], function (buster, agentvm, moment) {
	return function () {

		buster.testCase("Rta Tool agentviewmodel", {
			"should create viewmodel": function () {

				var vm = new agentvm();
				assert(vm);
			},
			"should state basic info for state": function () {

				var actualState = {};
				var vm = new agentvm(function (state) {
					actualState = state;
				});
				vm.fill({ name: '', usercode: '0097' });
				vm.sendState();

				expect(actualState.AuthenticationKey).toEqual(vm.AuthenticationKey);
				expect(actualState.UserCode).toEqual('0097');
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
				var vm = new agentvm(function (state) {
					actualState = state;
				});

				vm.answer();

				expect(actualState.StateCode).toEqual('InCall');
				expect(actualState.StateDescription).toEqual('InCall');

			},


			"should send ready call state when hang up": function () {

				var actualState = {};
				var vm = new agentvm(function (state) {
					actualState = state;
				});

				vm.hangUp();

				expect(actualState.StateCode).toEqual('Ready');
				expect(actualState.StateDescription).toEqual('Ready');

			},

			"should send specified state on send": function () {

				var actualState = {};
				var vm = new agentvm(function (state) {
					actualState = state;
				});

				vm.specifiedState("AUX1");
				vm.sendState();

				expect(actualState.StateCode).toEqual('AUX1');
				expect(actualState.StateDescription).toEqual('AUX1');
			},

			"should send logon when logging on": function () {
				var actualState = {};
				var vm = new agentvm(function (state) {
					actualState = state;
				});

				vm.logOn();

				expect(actualState.IsLoggedOn).toEqual(true);
				expect(actualState.StateCode).toEqual('Ready');
			},

			"should send logoff when logging off": function () {
				var actualState = {};
				var vm = new agentvm(function (state) {
					actualState = state;
				});

				vm.logOff();

				expect(actualState.IsLoggedOn).toEqual(false);
			},

			"should load statecodes" : function() {
				var expectedStateCodes = ['AUX1', 'Phone', 'AUX23'];
				var vm = new agentvm(function () {},function() { return expectedStateCodes; });

				for (var i = 0; i < expectedStateCodes.length; i++) {
					expect(vm.statecodes()[i].code()).toEqual(expectedStateCodes[i]);
				}
			},

			"should send statecode when executed" : function() {
				var expectedStateCodes = ['AUX1', 'Phone'];
				var actualState = {};

				var vm = new agentvm(function (state) {
					actualState = state;
				}, function () { return expectedStateCodes; });

				vm.statecodes()[0].sendCode();
				expect(actualState.StateCode).toEqual('AUX1');

				vm.statecodes()[1].sendCode();
				expect(actualState.StateCode).toEqual('Phone');

			}


		});
	};
});