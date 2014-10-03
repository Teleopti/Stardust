define([
	'knockout',
	'moment',
	'rta'
], function(
	ko,
	moment,
	rta
) {

	return function (rtaServerCall) {

		rtaServerCall = rtaServerCall || rta.ServerCall;

		var self = this;


		self.AuthenticationKey = '!#¤atAbgT%'; 

		self.answer = function() {
			rtaServerCall(makeAgentState('InCall'));
		};

		self.hangUp = function () {
			rtaServerCall(makeAgentState('Ready'));
		};

		var makeAgentState = function(stateCode) {
			return {
				UserCode: '0085',
				StateCode: stateCode,
				StateDescription: stateCode,
				IsLoggedOn: true,
				SecondsInState: 0,
				TimeStamp: moment(),
				PlatformTypeId: '00000000-0000-0000-0000-000000000000',
				SourceId: 1,
				BatchId: moment(),
				IsSnapshot: false
			}
		}

	};
});

