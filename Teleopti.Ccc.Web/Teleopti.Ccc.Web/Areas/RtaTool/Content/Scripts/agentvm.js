define([
	'knockout',
	'moment',
	'rta',
	'statecodevm'
], function (
	ko,
	moment,
	rta,
	statecodevm
) {

	return function (rtaServerCall, rtaStateCodes) {
		var self = this;

		self.name = ko.observable();
		self.usercode = ko.observable();
		self.authenticationKey = ko.observable();
		
		rtaServerCall = rtaServerCall || rta.ServerCall;
		rtaStateCodes = rtaStateCodes || rta.FetchStateCodes;

		self.statecodes = ko.observableArray();


		ko.utils.arrayForEach(rtaStateCodes(), function (data) {
			var svm = new statecodevm(data, function () {
				var state = makeAgentState(data.code, data.loggedon);
				rtaServerCall(state);
			});
			self.statecodes().push(svm);
		});

		self.fill = function (agent, authenticationKey) {
			self.name(agent.name);
			self.usercode(agent.usercode);
			self.authenticationKey = authenticationKey;
		}

		var makeAgentState = function (stateCode, isLoggedOn) {
			return  {
				AuthenticationKey: self.authenticationKey(),
				UserCode: self.usercode(),
				StateCode: stateCode,
				StateDescription: stateCode,
				IsLoggedOn: isLoggedOn,
				SecondsInState: 0,
				TimeStamp: moment.utc().format('YYYY-MM-DD HH:mm:ss'),
				PlatformTypeId: '00000000-0000-0000-0000-000000000000',
				SourceId: 1,
				BatchId: moment.utc().format('YYYY-MM-DD HH:mm:ss'),
				IsSnapshot: false
			};
		}
	};
});

