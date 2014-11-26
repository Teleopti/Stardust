define([
	'knockout',
	'moment',
	'rta'
], function (
	ko,
	moment,
	rta
) {

	return function (rtaServerCall, rtaStateCodes) {

		var statecodevm = function(code,send) {
			var that = this;
			that.code = ko.observable(code);
			that.sendCode = send;

			return that;
		};

		var self = this;

		rtaServerCall = rtaServerCall || rta.ServerCall;
		rtaStateCodes = rtaStateCodes || rta.FetchStateCodes;

		self.statecodes = ko.observableArray();

		ko.utils.arrayForEach(rtaStateCodes(), function (data) {
			var svm = new statecodevm(data,function() {
				var state = makeAgentState(data, true);
				rtaServerCall(state);
			});
			self.statecodes().push(svm);
		});

		self.AuthenticationKey = '!#¤atAbgT%';

		self.answer = function () {
			rtaServerCall(makeAgentState('InCall', true));
		};

		self.hangUp = function () {
			rtaServerCall(makeAgentState('Ready', true));
		};

		self.specifiedState = ko.observable('');

		self.sendState = function () {
			rtaServerCall(makeAgentState(self.specifiedState(), true));
		}

		self.logOn = function () {
			makeAgentState('Ready', true);
			rtaServerCall(makeAgentState('Ready', true));
		}

		self.logOff = function () {
			rtaServerCall(makeAgentState('', false));
		}

		self.name = ko.observable();

		self.usercode = ko.observable();

		self.fill = function(agent) {
			self.name(agent.name);
			self.usercode(agent.usercode);
		}

		self.selected = ko.observable(false);

		var makeAgentState = function (stateCode, isLoggedOn) {
			return {
				AuthenticationKey: self.AuthenticationKey,
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
			}
		}
	};
});

