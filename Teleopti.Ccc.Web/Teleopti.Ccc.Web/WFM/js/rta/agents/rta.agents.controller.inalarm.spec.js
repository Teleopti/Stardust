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

		scope = $controllerBuilder.setup('RtaAgentsCtrl');

		$fakeBackend.clear();

	}));

	it('should display states in alarm only', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend.withAgent({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withAgent({
				Name: "Charley Caper",
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				State: "Break",
				TimeInAlarm: 0
			})
			.withState({
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				State: "Break",
				TimeInAlarm: 60
			});

		$controllerBuilder.createController()
			.apply('agentsInAlarm = true');

		expect(scope.filteredData.length).toEqual(1);
		expect(scope.filteredData[0].Name).toEqual("Charley Caper");
		expect(scope.filteredData[0].State).toEqual("Break");
	});

	it('should display states in alarm only for site', function () {
		stateParams.siteId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend.withAgent({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				SiteId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withAgent({
				Name: "Charley Caper",
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				SiteId: "34590a63-6331-4921-bc9f-9b5e015ab495",
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				State: "Break",
				TimeInAlarm: 0
			})
			.withState({
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				State: "Break",
				TimeInAlarm: 60
			});

		$controllerBuilder.createController()
			.apply('agentsInAlarm = true');

		expect(scope.filteredData.length).toEqual(1);
		expect(scope.filteredData[0].Name).toEqual("Charley Caper");
		expect(scope.filteredData[0].State).toEqual("Break");
	});

	it('should display nothing', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend.withAgent({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				State: "Break",
				TimeInAlarm: 0
			});

		$controllerBuilder.createController()
			.apply('agentsInAlarm = true');

		expect(scope.filteredData.length).toEqual(0);
	});

	it('should display states with alarm time in desc order when agentsInAlarm is turned on', function () {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend.withAgent({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withAgent({
				Name: "Charley Caper",
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TimeInAlarm: 30
			})
			.withState({
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				TimeInAlarm: 60
			});

		$controllerBuilder.createController()
			.apply('agentsInAlarm = true');

		expect(scope.filteredData[0].Name).toEqual("Charley Caper");
		expect(scope.filteredData[1].Name).toEqual("Ashley Andeen");
	});

	it('should show all agents if it is specified by url', function() {
		stateParams.showAllAgents = true;

		$controllerBuilder.createController();

		expect(scope.agentsInAlarm).toEqual(false);
	});

	it('should set bool to indicate if user opened max number of agents', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";

		for (var i = 0; i < 50; i++) {
			$fakeBackend.withAgent({
				PersonId: i,
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withState({
				PersonId: i,
				TimeInAlarm: 30
			});
		}
		$controllerBuilder.createController()
			.apply('agentsInAlarm = true');

		expect(scope.openedMaxNumberOfAgents).toEqual(true);
	});

	it('should not set bool to indicate if user opened max number of agents', function () {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";

		$fakeBackend.withAgent({
			PersonId: "1",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
		})
			.withState({
				PersonId: "1",
				TimeInAlarm: 30
			});
		$controllerBuilder.createController()
			.apply('agentsInAlarm = true');

		expect(scope.openedMaxNumberOfAgents).toEqual(false);
	});
});
