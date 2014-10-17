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
		var self = this;
		rtaServerCall = rtaServerCall || rta.ServerCall;

		self.AuthenticationKey = '!#¤atAbgT%';

		self.answer = function() {
			rtaServerCall(makeAgentState('InCall'));
		};

		self.hangUp = function () {
			rtaServerCall(makeAgentState('Ready'));
		};

		self.specifiedState = ko.observable('');

		self.sendState = function() {
			rtaServerCall(makeAgentState(self.specifiedState()));
		}

		self.isLoggedOn = ko.observable(true);

		var makeAgentState = function(stateCode) {
			return {
				AuthenticationKey: self.AuthenticationKey,
				UserCode: '0085',
				StateCode: stateCode,
				StateDescription: stateCode,
				IsLoggedOn: self.isLoggedOn(),
				SecondsInState: 0,
				TimeStamp: moment.utc().format('YYYY-MM-DD HH:mm:ss'),
				PlatformTypeId: '00000000-0000-0000-0000-000000000000',
				SourceId: 1,
				BatchId: moment.utc().format('YYYY-MM-DD HH:mm:ss'),
				IsSnapshot: false
			}
		}

	};
});

