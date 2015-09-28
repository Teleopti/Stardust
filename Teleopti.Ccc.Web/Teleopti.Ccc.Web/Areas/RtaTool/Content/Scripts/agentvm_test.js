define(['buster', 'agentvm', 'moment'], function (buster, agentvm, moment) {
	return function () {

		buster.testCase("Rta Tool agentviewmodel", {
			"should create viewmodel": function () {

				var vm = new agentvm();
				assert(vm);
			},
			"should state basic info for state": function () {

				var actualState = {};
				var states = [{ code: 'AUX1', loggedon: true }];
				var vm = new agentvm(function (state) {
					actualState = state;
				},
					function () { return states; });

				vm.fill({ name: '', usercode: '0097' }, function() { return '!#¤atAbgT%'; });
				vm.statecodes()[0].sendState();

				expect(actualState.AuthenticationKey).toEqual('!#¤atAbgT%');
				expect(actualState.UserCode).toEqual('0097');
				expect(actualState.IsLoggedOn).toEqual(true);
				expect(actualState.SecondsInState).toEqual(0);
				expect(moment(actualState.TimeStamp).isValid()).toBe(true);
				expect(actualState.PlatformTypeId).toEqual('00000000-0000-0000-0000-000000000000');
				expect(actualState.SourceId).toEqual(1);
				expect(moment(actualState.BatchId).isValid()).toBe(true);
				expect(actualState.IsSnapshot).toBe(false);

			},
	

			"should load statecodes" : function() {
				var expectedStateCodes = [{ code : 'AUX1' }, { code : 'Phone' }, { code : 'AUX23' }];
				var vm = new agentvm(function () {},function() { return expectedStateCodes; });

				for (var i = 0; i < expectedStateCodes.length; i++) {
					expect(vm.statecodes()[i].code()).toEqual(expectedStateCodes[i].code);
				}
			},

			"should send selected statecode" : function() {
				var expectedStateCodes = [
					{ code: 'AUX1', loggedon: true},
					{ code: 'Phone', loggedon: false }];

				var actualState = {};

				var vm = new agentvm(function (state) {
					actualState = state;
				}, function() {
					return expectedStateCodes;
				});

				vm.statecodes()[0].sendState();
				expect(actualState.StateCode).toEqual('AUX1');
				expect(actualState.IsLoggedOn).toEqual(true);

				vm.statecodes()[1].sendState();
				expect(actualState.StateCode).toEqual('Phone');
				expect(actualState.IsLoggedOn).toEqual(false);

			}

		});
	};
});