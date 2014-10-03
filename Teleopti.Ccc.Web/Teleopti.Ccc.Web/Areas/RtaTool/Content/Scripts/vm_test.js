define(['buster','vm','moment'], function (buster,viewmodel,moment) {
	return function() {
		buster.testCase("Diagnosis viewmodel", {
			"should create viewmodel": function() {

				var vm = new viewmodel();
				assert(vm);
			},

			"=>should send in call state when answer": function () {


				var actualState = {};

				var callback = function(state) {
					actualState = state;
				};

				var vm = new viewmodel(callback);

				vm.answer();


				expect(actualState.AuthenticationKey).toEqual('!#¤atAbgT%'); 
				expect(actualState.UserCode).toEqual('0085');
				expect(actualState.StateCode).toEqual('InCall');
				expect(actualState.StateDescription).toEqual('InCall');
				expect(actualState.IsLoggedOn).toEqual(true);
				expect(actualState.SecondsInState).toEqual(0);
				expect(moment(actualState.TimeStamp).isValid()).toBe(true);
				expect(actualState.PlatformTypeId).toEqual('00000000-0000-0000-0000-000000000000');
				expect(actualState.SourceId).toEqual(1);
				expect(moment(actualState.BatchId).isValid()).toBe(true);
				expect(actualState.IsSnapshot).toBe(false);

			}
	});
	};
});