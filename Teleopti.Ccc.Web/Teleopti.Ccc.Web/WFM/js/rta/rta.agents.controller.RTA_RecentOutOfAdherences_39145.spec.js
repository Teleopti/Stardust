'use strict';
describe('RtaAgentsCtrl', function() {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		$fakeBackend,
		$controllerBuilder,
		scope;

	var stateParams = {};

	beforeEach(module('wfm.rta'));

	beforeEach(function() {
		module(function($provide) {
			$provide.service('$stateParams', function() {
				stateParams = {};
				return stateParams;
			});
		});
	});

	beforeEach(inject(function(_$httpBackend_, _$interval_, _$state_, _$sessionStorage_, _FakeRtaBackend_, _ControllerBuilder_) {
		$interval = _$interval_;
		$state = _$state_;
		$sessionStorage = _$sessionStorage_;
		$httpBackend = _$httpBackend_;
		$fakeBackend = _FakeRtaBackend_;
		$controllerBuilder = _ControllerBuilder_;

		scope = $controllerBuilder.setup('RtaAgentsCtrl');
		$fakeBackend.withToggle('RTA_SeeRecentOutOfAdherenceOccurancesToday_39145');

		$fakeBackend.clear();

	}));

	var minutesToPercent = function(minutes) {
		return (minutes * (25 / 60)) + "%";
	}

	it('should display out of adherence', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend
			.withAgent({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withTime('2016-06-15 08:00')
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				OutOfAdherences: [{
					StartTime: "2016-06-15 07:30:00",
					EndTime: "2016-06-15 07:45:00"
				}]
			});

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false');

		expect(scope.agents[0].OutOfAdherences[0].Offset).toEqual(minutesToPercent(30));
		expect(scope.agents[0].OutOfAdherences[0].Width).toEqual(minutesToPercent(15));
	});

	it('should display past out of adherences', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend
			.withAgent({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withTime('2016-06-15 08:00')
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				OutOfAdherences: [{
					StartTime: "2016-06-15 07:15:00",
					EndTime: "2016-06-15 07:30:00"
				}, {
					StartTime: "2016-06-15 07:45:00",
					EndTime: "2016-06-15 08:00:00"
				}]
			});

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false');

		expect(scope.agents[0].OutOfAdherences[0].Offset).toEqual(minutesToPercent(15));
		expect(scope.agents[0].OutOfAdherences[0].Width).toEqual(minutesToPercent(15));
		expect(scope.agents[0].OutOfAdherences[1].Offset).toEqual(minutesToPercent(45));
		expect(scope.agents[0].OutOfAdherences[1].Width).toEqual(minutesToPercent(15));
	});

	it('should display on going out of adherence', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend
			.withAgent({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withTime('2016-06-15 08:00')
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				OutOfAdherences: [{
					StartTime: "2016-06-15 07:45:00",
					EndTime: null
				}]
			});

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false');

		expect(scope.agents[0].OutOfAdherences[0].Offset).toEqual(minutesToPercent(45));
		expect(scope.agents[0].OutOfAdherences[0].Width).toEqual(minutesToPercent(15));
	});

	it('should cut on going out of adherence', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend
			.withAgent({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withTime('2016-06-15 08:00')
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				OutOfAdherences: [{
					StartTime: "2016-06-15 06:45:00",
					EndTime: null
				}]
			});

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false');

		expect(scope.agents[0].OutOfAdherences[0].Offset).toEqual(minutesToPercent(0));
		expect(scope.agents[0].OutOfAdherences[0].Width).toEqual(minutesToPercent(60));
	});

	it('should display on going out of adherence time', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend
			.withAgent({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withTime('2016-06-15 08:00')
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				OutOfAdherences: [{
					StartTime: "2016-06-15 07:50:00",
					EndTime: null
				}]
			});

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false');

		expect(scope.agents[0].TimeOutOfAdherence).toEqual("0:10:00");
	});

	it('should not display out of adherence time when in adherence', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend
			.withAgent({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withTime('2016-06-15 08:00')
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				OutOfAdherences: [{
					StartTime: "2016-06-15 07:15:00",
					EndTime: "2016-06-15 07:30:00"
				}]
			});

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false');

		expect(scope.agents[0].TimeOutOfAdherence).toEqual(undefined);
	});

	it('should display when out of adherence started and ended', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend
			.withAgent({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withTime('2016-06-15 08:00')
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				OutOfAdherences: [{
					StartTime: "2016-06-15 07:30:00",
					EndTime: "2016-06-15 07:45:00"
				}]
			});

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false');

		expect(scope.agents[0].OutOfAdherences[0].StartTime).toEqual("07:30:00");
		expect(scope.agents[0].OutOfAdherences[0].EndTime).toEqual("07:45:00");
	});

	it('should display when ongoing out of adherence started', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend
			.withAgent({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withTime('2016-06-15 08:00')
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				OutOfAdherences: [{
					StartTime: "2016-06-15 07:30:00",
					EndTime: null
				}]
			});

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false');

		expect(scope.agents[0].OutOfAdherences[0].StartTime).toEqual("07:30:00");
		expect(scope.agents[0].OutOfAdherences[0].EndTime).toEqual(null);
	});


});
