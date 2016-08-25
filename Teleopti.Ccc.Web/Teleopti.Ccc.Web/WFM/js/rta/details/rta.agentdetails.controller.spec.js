'use strict';
describe('RtaAgentDetailsCtrl', function() {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		$fakeBackend,
		$controllerBuilder,
		scope;

	var stateParams = {};

	beforeEach(module('wfm.rta'));

	beforeEach(function () {
		module(function ($provide) {
			$provide.service('$stateParams', function () {
				stateParams = {};
				return stateParams;
			});
		});
	});

	beforeEach(inject(function (_$httpBackend_, _$interval_, _$state_, _$sessionStorage_, _FakeRtaBackend_, _ControllerBuilder_) {
		$interval = _$interval_;
		$state = _$state_;
		$sessionStorage = _$sessionStorage_;
		$httpBackend = _$httpBackend_;
		$fakeBackend = _FakeRtaBackend_;
		$controllerBuilder = _ControllerBuilder_;

		scope = $controllerBuilder.setup('RtaAgentDetailsCtrl');

		$fakeBackend.clear();
	}));

	it('should get name for agent', function() {
		stateParams.personId = "11610fe4-0130-4568-97de-9b5e015b2564";
		$fakeBackend.withPersonDetails({
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			Name: "Ashley Andeen"
		});

		$controllerBuilder.createController();

		expect(scope.name).toEqual("Ashley Andeen");
	});

	it('should get details for agent', function() {
		stateParams.personId = "11610fe4-0130-4568-97de-9b5e015b2564";
		$fakeBackend.withActivityAdherence({
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			Name: "Phone",
			StartTime: "2014-10-06T08:00:00",
			ActualStartTime: "2014-10-06T08:00:00",
			TimeInAdherence: "00:30:00",
			TimeOutOfAdherence: "01:30:00"
		});

		$controllerBuilder.createController();

		expect(scope.adherence[0].Name).toEqual("Phone");
		expect(scope.adherence[0].StartTime).toEqual("2014-10-06T08:00:00");
		expect(scope.adherence[0].ActualStartTime).toEqual("2014-10-06T08:00:00");
		expect(scope.adherence[0].TimeInAdherence).toEqual("00:30:00");
		expect(scope.adherence[0].TimeOutOfAdherence).toEqual("01:30:00");
	});
});
